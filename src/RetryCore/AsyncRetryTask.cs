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
internal class AsyncRetryTask : IAsyncRetriable
{
    private IAsyncRetriable<int> _retryTask;

    public AsyncRetryTask(Func<Task> function, RetryOptions retryOptions, Type[] retryExceptions)
    {
        _retryTask = new AsyncRetryTask<int>(async () =>
        {
            await function();
            return 1;
        }, retryOptions, retryExceptions);
    }

    /// <inheritdoc />
    public async Task<RetryResult> RunAsync(CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        return await _retryTask.RunAsync(cancellationToken, continueOnCapturedContext);
    }

    /// <inheritdoc />
    public IAsyncRetriable OnRetry(Action<RetryResult> retryAction)
    {
        _retryTask = _retryTask.OnRetry(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnRetry(Action<RetryResult, int> retryAction)
    {
        _retryTask = _retryTask.OnRetry(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnRetryAsync(Func<RetryResult, Task> retryAction)
    {
        _retryTask = _retryTask.OnRetryAsync(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnRetryAsync(Func<RetryResult, int, Task> retryAction)
    {
        _retryTask = _retryTask.OnRetryAsync(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnSuccess(Action<RetryResult> successAction)
    {
        _retryTask = _retryTask.OnSuccess(successAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnSuccess(Action<RetryResult, int> successAction)
    {
        _retryTask = _retryTask.OnSuccess(successAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnSuccessAsync(Func<RetryResult, Task> successAction)
    {
        _retryTask = _retryTask.OnSuccessAsync(successAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnSuccessAsync(Func<RetryResult, int, Task> successAction)
    {
        _retryTask = _retryTask.OnSuccessAsync(successAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnFailure(Action<RetryResult> failureAction)
    {
        _retryTask = _retryTask.OnFailure(failureAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnFailure(Action<RetryResult, int> failureAction)
    {
        _retryTask = _retryTask.OnFailure(failureAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnFailureAsync(Func<RetryResult, Task> failureAction)
    {
        _retryTask = _retryTask.OnFailureAsync(failureAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnFailureAsync(Func<RetryResult, int, Task> failureAction)
    {
        _retryTask = _retryTask.OnFailureAsync(failureAction);

        return this;
    }
}

/// <summary>
/// Represents the task to be retried.
/// </summary>
/// <typeparam name="T">The type of result returned by the retried delegate.</typeparam>
internal class AsyncRetryTask<T> : IAsyncRetriable<T>
{
    private readonly Func<Task<T>> _taskToTry;

    private readonly Type[] _retryExceptions;

    private readonly RetryOptions _retryOptions;

    public AsyncRetryTask(Func<Task<T>> function, RetryOptions retryOptions, Type[] retryExceptions)
    {
        _taskToTry = function;
        _retryOptions = retryOptions;
        _retryExceptions = retryExceptions;

        _retryActions = new List<Delegate>();
        _successActions = new List<Delegate>();
        _failureActions = new List<Delegate>();
    }

    private readonly List<Delegate> _retryActions;

    private readonly List<Delegate> _successActions;

    private readonly List<Delegate> _failureActions;

    private CancellationToken _cancellationToken;

    /// <inheritdoc />
    public async Task<RetryResult<T>> RunAsync(CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false)
    {
        //start
        _cancellationToken = cancellationToken;
        return await TryImplAsync().ConfigureAwait(continueOnCapturedContext);

        //end
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnRetry(Action<RetryResult<T>> retryAction)
    {
        return OnRetry((result, _) => retryAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnRetry(Action<RetryResult<T>, int> retryAction)
    {
        _retryActions.Add(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnRetryAsync(Func<RetryResult<T>, Task> retryAction)
    {
        return OnRetryAsync((result, _) => retryAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnRetryAsync(Func<RetryResult<T>, int, Task> retryAction)
    {
        _retryActions.Add(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnSuccess(Action<RetryResult<T>> successAction)
    {
        return OnSuccess((result, _) => successAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnSuccess(Action<RetryResult<T>, int> successAction)
    {
        _successActions.Add(successAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnSuccessAsync(Func<RetryResult<T>, Task> successAction)
    {
        return OnSuccessAsync((result, _) => successAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnSuccessAsync(Func<RetryResult<T>, int, Task> successAction)
    {
        _successActions.Add(successAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnFailure(Action<RetryResult<T>> failureAction)
    {
        return OnFailure((result, _) => failureAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnFailure(Action<RetryResult<T>, int> failureAction)
    {
        _failureActions.Add(failureAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnFailureAsync(Func<RetryResult<T>, Task> failureAction)
    {
        return OnFailureAsync((result, _) => failureAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnFailureAsync(Func<RetryResult<T>, int, Task> failureAction)
    {
        _failureActions.Add(failureAction);

        return this;
    }

    private async Task<RetryResult<T>> TryImplAsync()
    {
        //TraceSource.TraceVerbose("Starting trying with max try time {0} and max try count {1}.",
        //    MaxTryTime, MaxTryCount);
        int triedCount = 0;
        var sw = Stopwatch.StartNew();

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
                    if (_retryOptions.TryInterval.Ticks > 0)
                    {
                        await Task.Delay(_retryOptions.TryInterval, _cancellationToken);
                    }

                    await Retry(result, triedCount);
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
                result.Result = await _taskToTry();
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

            //onsuccess
            try
            {
                await Success(result, triedCount);
            }
            catch (Exception ex)
            {
                result.Exception = new SuccessCallbackException(ex);
                break;
            }

            return result;
        } while (ShouldContinue(result, sw.Elapsed, triedCount)); //onretry

        //failure
        try
        {
            await Failure(result, triedCount);
        }
        catch (Exception ex)
        {
            result.Exception = new FailureCallbackException(ex);
        }

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
        if (_retryExceptions.Any() && _retryExceptions.All(o => o.IsInstanceOfType(exception)))
        {
            return false;
        }

        return true;
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

        if (_cancellationToken.IsCancellationRequested)
        {
            result.Exception = new TaskCanceledException();
            return false;
        }

        return true;
    }

    private Task Success(RetryResult<T> result, int triedCount)
    {
        return InvokeActions(_successActions, result, triedCount);
    }

    private Task Retry(RetryResult<T> result, int triedCount)
    {
        return InvokeActions(_retryActions, result, triedCount);
    }

    private Task Failure(RetryResult<T> result, int triedCount)
    {
        return InvokeActions(_failureActions, result, triedCount);
    }

    private static async Task InvokeActions(IEnumerable<Delegate> actions, RetryResult<T> result, int triedCount)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case Action<RetryResult<T>, int> sync:
                    sync(result, triedCount);
                    break;
                case Func<RetryResult<T>, int, Task> async:
                    await async(result, triedCount);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
