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
    IRetriable IRetriable.OnRetry(Action<RetryResult, RetryContext> retryAction)
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
    IRetriable IRetriable.OnSuccess(Action<RetryResult, RetryContext> successAction)
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
    IRetriable IRetriable.OnFailure(Action<RetryResult, RetryContext> failureAction)
    {
        _retryTask = _retryTask.OnFailure(failureAction);

        return this;
    }
}

/// <summary>
/// Represents the task to be retried.
/// </summary>
/// <typeparam name="T">The type of result returned by the retried delegate.</typeparam>
internal class RetryTask<T> : IRetriable<T>
{
    private readonly Type[] _retryExceptions;

    private readonly RetryOptions _retryOptions;

    private readonly List<Action<RetryResult<T>, RetryContext>> _retryActions;

    private readonly List<Action<RetryResult<T>, RetryContext>> _successActions;

    private readonly List<Action<RetryResult<T>, RetryContext>> _failureActions;

    private readonly Func<T> _taskToTry;

    private Func<RetryResult<T>, bool>? _condition;

    public RetryTask(Func<T> function, RetryOptions retryOptions, Type[] retryExceptions)
    {
        _taskToTry = function;
        _retryOptions = retryOptions;
        _retryExceptions = retryExceptions;

        _retryActions = new List<Action<RetryResult<T>, RetryContext>>();
        _successActions = new List<Action<RetryResult<T>, RetryContext>>();
        _failureActions = new List<Action<RetryResult<T>, RetryContext>>();
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
    public IRetriable<T> OnRetry(Action<RetryResult<T>, RetryContext> retryAction)
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
    public IRetriable<T> OnSuccess(Action<RetryResult<T>, RetryContext> successAction)
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
    public IRetriable<T> OnFailure(Action<RetryResult<T>, RetryContext> failureAction)
    {
        _failureActions.Add(failureAction);

        return this;
    }

    private RetryResult<T> TryImpl()
    {
        //TraceSource.TraceVerbose("Starting trying with max try time {0} and max try count {1}.",
        //    MaxTryTime, MaxTryCount);
        var sw = Stopwatch.StartNew();
        ManualResetEventSlim? resetEvent = null;
        var context = new RetryContext(0, 0, sw.Elapsed);

        // Start the try loop.
        var result = new RetryResult<T>();
        do
        {
            context.TriedCount++;
            //TraceSource.TraceVerbose("Trying time {0}, elapsed time {1}.", TriedCount, Stopwatch.Elapsed);

            if (context.TriedCount > 1)
            {
                try
                {
                    context.RetryCount = context.TriedCount - 1;
                    Retry(result, context);

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
                Success(result, context);
            }
            catch (Exception ex)
            {
                result.Exception = new SuccessCallbackException(ex);
                break;
            }

            goto Exit;
        } while (ShouldContinue(result, sw.Elapsed, context)); //onretry

        //failure
        try
        {
            Failure(result, context);
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

    private bool ShouldContinue(RetryResult<T> result, TimeSpan triedTime, RetryContext context)
    {
        context.TriedTime = triedTime;

        if (context.TriedTime >= _retryOptions.MaxTryTime)
        {
            result.Exception = new OverMaxTryTimeException(context.TriedTime);
            return false;
        }
        if (context.TriedCount >= _retryOptions.MaxTryCount)
        {
            result.Exception = new OverMaxTryCountException(context.TriedCount);
            return false;
        }

        return true;
    }

    private void Success(RetryResult<T> result, RetryContext context)
    {
        InvokeActions(_successActions, result, context);
    }

    private void Retry(RetryResult<T> result, RetryContext context)
    {
        InvokeActions(_retryActions, result, context);
    }

    private void Failure(RetryResult<T> result, RetryContext context)
    {
        InvokeActions(_failureActions, result, context);
    }

    private static void InvokeActions(IEnumerable<Action<RetryResult<T>, RetryContext>> actions, RetryResult<T> result,
        RetryContext context)
    {
        var clone = context.Clone();

        foreach (var action in actions)
        {
            action(result, clone);
        }
    }
}
