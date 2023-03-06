using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Retry.Exceptions;

namespace Retry;

/// <summary>
/// Represents the task to be retried.
/// </summary>
internal class RetryTask : IRetriable
{
    private readonly Action _action;

    private readonly Type[] _retryExceptions;

    private readonly RetryOptions _retryOptions;

    private IRetriable<int> _retryTask;

    public RetryTask(Action action, RetryOptions retryOptions, Type[] retryExceptions)
    {
        _action = action;
        _retryOptions = retryOptions;
        _retryExceptions = retryExceptions;

        _retryTask = new RetryTask<int>(() =>
        {
            _action();
            return 1;
        }, _retryOptions, _retryExceptions);
    }

    /// <inheritdoc />
    public IAsyncRetriable AsAsync()
    {
        return new AsyncRetryTask(() => Task.Run(_action), _retryOptions, _retryExceptions);
    }

    /// <inheritdoc />
    public RetryResult Run()
    {
        return _retryTask.Run();
    }

    /// <inheritdoc />
    public RetryResult Run(Func<RetryResult, bool> condition)
    {
        return _retryTask.Run(condition);
    }

    /// <inheritdoc />
    IRetriable IRetriable.OnRetry(Action<RetryResult> retryAction)
    {
        _retryTask = _retryTask.OnRetry(retryAction);

        return this;
    }

    /// <inheritdoc />
    IRetriable IRetriable.OnRetry(Action<RetryResult, int> retryAction)
    {
        _retryTask = _retryTask.OnRetry(retryAction);

        return this;
    }

    /// <inheritdoc />
    IRetriable IRetriable.OnSuccess(Action<RetryResult> successAction)
    {
        _retryTask = _retryTask.OnSuccess(successAction);

        return this;
    }

    /// <inheritdoc />
    IRetriable IRetriable.OnSuccess(Action<RetryResult, int> successAction)
    {
        _retryTask = _retryTask.OnSuccess(successAction);

        return this;
    }

    /// <inheritdoc />
    IRetriable IRetriable.OnFailure(Action<RetryResult> failureAction)
    {
        _retryTask = _retryTask.OnRetry(failureAction);

        return this;
    }

    /// <inheritdoc />
    IRetriable IRetriable.OnFailure(Action<RetryResult, int> failureAction)
    {
        _retryTask = _retryTask.OnFailure(failureAction);

        return this;
    }

    ///// <inheritdoc />
    //public IRetriable Assert(Func<RetryResult, bool> condition)
    //{
    //    _retryTask = _retryTask.Assert(condition);

    //    return this;
    //}
}

/// <summary>
/// Represents the task to be retried.
/// </summary>
/// <typeparam name="T">The type of result returned by the retried delegate.</typeparam>
internal class RetryTask<T> : IRetriable<T>
{
    private readonly List<Action<RetryResult<T>, int>> _failureActions;

    private readonly List<Action<RetryResult<T>, int>> _retryActions;

    private readonly Type[] _retryExceptions;

    private readonly RetryOptions _retryOptions;

    private readonly List<Action<RetryResult<T>, int>> _successActions;

    private readonly Func<T> _taskToTry;

    private Func<RetryResult<T>, bool>? _condition;

    public RetryTask(Func<T> function, RetryOptions retryOptions, Type[] retryExceptions)
    {
        _taskToTry = function;
        _retryOptions = retryOptions;
        _retryExceptions = retryExceptions;

        _retryActions = new List<Action<RetryResult<T>, int>>();
        _successActions = new List<Action<RetryResult<T>, int>>();
        _failureActions = new List<Action<RetryResult<T>, int>>();
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> AsAsync()
    {
        return new AsyncRetryTask<T>(() => Task.Run(_taskToTry), _retryOptions, _retryExceptions);
    }

    /// <inheritdoc />
    public RetryResult<T> Run()
    {
        return TryImpl();
    }

    /// <inheritdoc />
    public RetryResult<T> Run(Func<RetryResult<T>, bool> assert)
    {
        _condition = assert;

        return Run();
    }

    /// <inheritdoc />
    public IRetriable<T> OnRetry(Action<RetryResult<T>> retryAction)
    {
        return OnRetry((result, _) => retryAction(result));
    }

    /// <inheritdoc />
    public IRetriable<T> OnRetry(Action<RetryResult<T>, int> retryAction)
    {
        _retryActions.Add(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IRetriable<T> OnSuccess(Action<RetryResult<T>> successAction)
    {
        return OnSuccess((result, _) => successAction(result));
    }

    /// <inheritdoc />
    public IRetriable<T> OnSuccess(Action<RetryResult<T>, int> successAction)
    {
        _successActions.Add(successAction);

        return this;
    }

    /// <inheritdoc />
    public IRetriable<T> OnFailure(Action<RetryResult<T>> failureAction)
    {
        return OnFailure((result, _) => failureAction(result));
    }

    /// <inheritdoc />
    public IRetriable<T> OnFailure(Action<RetryResult<T>, int> failureAction)
    {
        _failureActions.Add(failureAction);

        return this;
    }

    ///// <inheritdoc />
    //public IRetriable<T> Assert(Func<RetryResult<T>, bool> condition)
    //{
    //    _condition = condition;

    //    return this;
    //}

    private RetryResult<T> TryImpl()
    {
        //TraceSource.TraceVerbose("Starting trying with max try time {0} and max try count {1}.",
        //    MaxTryTime, MaxTryCount);
        int triedCount = 0;
        var sw = Stopwatch.StartNew();
        ManualResetEventSlim? resetEvent = null;

        // Start the try loop.
        var result = new RetryResult<T>();
        do
        {
            triedCount++;
            //TraceSource.TraceVerbose("Trying time {0}, elapsed time {1}.", TriedCount, Stopwatch.Elapsed);

            if (triedCount > 1)
            {
                try
                {
                    Retry(result, triedCount - 1);

                    var delay = _retryOptions.RetryInterval.GetInterval();
                    if (delay.Ticks > 0)
                    {
                        resetEvent ??= new ManualResetEventSlim();
                        resetEvent.Wait(delay);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    result.Exception = ex;
                    break;
                }
                catch (Exception ex)
                {
                    result.Exception = new RetryCallbackException(ex);
                    break;
                }
            }

            try
            {
                // Perform the try action.
                result.Result = _taskToTry();
            }
            catch (Exception ex)
            {
                //can retry
                if (IsRetryException(ex))
                {
                    continue;
                }

                result.Exception = ex;
                break;
            }

            //assert
            try
            {
                if (AssertThenRetry(result))
                {
                    continue;
                }
            }
            catch (Exception ex)
            {
                result.Exception = new AssertCallbackException(ex);
                break;
            }

            //onsuccess
            try
            {
                Success(result, triedCount);
            }
            catch (Exception ex)
            {
                result.Exception = new SuccessCallbackException(ex);
                break;
            }

            goto Exit;
        } while (ShouldContinue(result, sw.Elapsed, triedCount)); //onretry

        //failure
        try
        {
            Failure(result, triedCount);
        }
        catch (Exception ex)
        {
            result.Exception = new FailureCallbackException(ex);
        }

        Exit:
        resetEvent?.Dispose();
        return result;
    }

    private bool IsRetryException(Exception exception)
    {
        // If exception is not recoverable,
        if (exception is OutOfMemoryException || exception is AccessViolationException)
        {
            return false;
        }

        // or exception is not retry exceptions.
        if (_retryExceptions.Any() && _retryExceptions.All(o => !o.IsInstanceOfType(exception)))
        {
            return false;
        }

        return true;
    }

    private bool AssertThenRetry(RetryResult<T> result)
    {
        var stop = _condition?.Invoke(result) ?? true;

        return !stop;
    }

    private bool ShouldContinue(RetryResult<T> result, TimeSpan triedTime, int triedCount)
    {
        if (triedTime >= _retryOptions.MaxTryTime)
        {
            result.Exception = new OverMaxTryTimeException(triedTime);
            return false;
        }
        if (triedCount >= _retryOptions.MaxTryCount)
        {
            result.Exception = new OverMaxTryCountException(triedCount);
            return false;
        }

        return true;
    }

    private void Success(RetryResult<T> result, int triedCount)
    {
        InvokeActions(_successActions, result, triedCount);
    }

    private void Retry(RetryResult<T> result, int retryCount)
    {
        InvokeActions(_retryActions, result, retryCount);
    }

    private void Failure(RetryResult<T> result, int triedCount)
    {
        InvokeActions(_failureActions, result, triedCount);
    }

    private static void InvokeActions(IEnumerable<Action<RetryResult<T>, int>> actions, RetryResult<T> result,
        int triedCount)
    {
        foreach (var action in actions)
        {
            action(result, triedCount);
        }
    }
}
