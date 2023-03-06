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
    public async Task<RetryResult> RunAsync(Func<RetryResult, bool> condition,
        CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false)
    {
        return await _retryTask.RunAsync(condition, cancellationToken, continueOnCapturedContext);
    }

    /// <inheritdoc />
    public async Task<RetryResult> RunAsync(Func<RetryResult, Task<bool>> condition,
        CancellationToken cancellationToken = default, bool continueOnCapturedContext = false)
    {
        return await _retryTask.RunAsync(condition, cancellationToken, continueOnCapturedContext);
    }

    /// <inheritdoc />
    public IAsyncRetriable OnRetry(Action<RetryResult> retryAction)
    {
        _retryTask = _retryTask.OnRetry(retryAction);

        return this;
    }

    /// <inheritdoc />
    public IAsyncRetriable OnRetry(Action<RetryResult, RetryContext> retryAction)
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
    public IAsyncRetriable OnRetryAsync(Func<RetryResult, RetryContext, Task> retryAction)
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
    public IAsyncRetriable OnSuccess(Action<RetryResult, RetryContext> successAction)
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
    public IAsyncRetriable OnSuccessAsync(Func<RetryResult, RetryContext, Task> successAction)
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
    public IAsyncRetriable OnFailure(Action<RetryResult, RetryContext> failureAction)
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
    public IAsyncRetriable OnFailureAsync(Func<RetryResult, RetryContext, Task> failureAction)
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

    private Func<RetryResult<T>, bool>? _condition;

    private Func<RetryResult<T>, Task<bool>>? _conditionTask;

    private CancellationToken _cancellationToken;

    /// <inheritdoc />
    public async Task<RetryResult<T>> RunAsync(CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false)
    {
        _cancellationToken = cancellationToken;

        return await TryImplAsync().ConfigureAwait(continueOnCapturedContext);
    }

    /// <inheritdoc />
    public Task<RetryResult<T>> RunAsync(Func<RetryResult<T>, bool> condition,
        CancellationToken cancellationToken = default, bool continueOnCapturedContext = false)
    {
        _condition = condition;
        _conditionTask = null;

        return RunAsync(cancellationToken, continueOnCapturedContext);
    }

    /// <inheritdoc />
    public Task<RetryResult<T>> RunAsync(Func<RetryResult<T>, Task<bool>> condition,
        CancellationToken cancellationToken = default, bool continueOnCapturedContext = false)
    {
        _condition = null;
        _conditionTask = condition;

        return RunAsync(cancellationToken, continueOnCapturedContext);
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnRetry(Action<RetryResult<T>> retryAction)
    {
        return OnRetry((result, _) => retryAction(result));
    }

    /// <inheritdoc />
    public IAsyncRetriable<T> OnRetry(Action<RetryResult<T>, RetryContext> retryAction)
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
    public IAsyncRetriable<T> OnRetryAsync(Func<RetryResult<T>, RetryContext, Task> retryAction)
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
    public IAsyncRetriable<T> OnSuccess(Action<RetryResult<T>, RetryContext> successAction)
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
    public IAsyncRetriable<T> OnSuccessAsync(Func<RetryResult<T>, RetryContext, Task> successAction)
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
    public IAsyncRetriable<T> OnFailure(Action<RetryResult<T>, RetryContext> failureAction)
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
    public IAsyncRetriable<T> OnFailureAsync(Func<RetryResult<T>, RetryContext, Task> failureAction)
    {
        _failureActions.Add(failureAction);

        return this;
    }

    private async Task<RetryResult<T>> TryImplAsync()
    {
        //TraceSource.TraceVerbose("Starting trying with max try time {0} and max try count {1}.",
        //    MaxTryTime, MaxTryCount);
        var sw = Stopwatch.StartNew();
        var context = new RetryContext(0, 0, sw.Elapsed);

        // Start the try loop.
        var result = new RetryResult<T>();
        do
        {
            context.TriedCount++;
            //TraceSource.TraceVerbose("Trying time {0}, elapsed time {1}.", TriedCount, Stopwatch.Elapsed);

            if (context.TriedCount > 1)
            {
                //on retry
                try
                {
                    context.RetryCount = context.TriedCount - 1;
                    await Retry(result, context);

                    var delay = _retryOptions.RetryInterval.GetInterval();
                    if (delay.Ticks > 0)
                    {
                        await Task.Delay(delay, _cancellationToken);
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

            //assert
            try
            {
                if (await AssertThenRetry(result))
                {
                    continue;
                }
            }
            catch (Exception ex)
            {
                result.Exception = new AssertCallbackException(ex);
                break;
            }

            //on success
            try
            {
                await Success(result, context);
            }
            catch (Exception ex)
            {
                result.Exception = new SuccessCallbackException(ex);
                break;
            }

            return result;
        } while (ShouldContinue(result, sw.Elapsed, context));

        //on failure
        try
        {
            await Failure(result, context);
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
        if (_retryExceptions.Any() && _retryExceptions.All(o => !o.IsInstanceOfType(exception)))
        {
            return false;
        }

        return true;
    }

    private async Task<bool> AssertThenRetry(RetryResult<T> result)
    {
        var stop = _condition?.Invoke(result) ?? true;
        if (_conditionTask is not null)
        {
            stop = await _conditionTask(result);
        }

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

        if (_cancellationToken.IsCancellationRequested)
        {
            result.Exception = new TaskCanceledException();
            return false;
        }

        return true;
    }

    private Task Success(RetryResult<T> result, RetryContext context)
    {
        return InvokeActions(_successActions, result, context);
    }

    private Task Retry(RetryResult<T> result, RetryContext context)
    {
        return InvokeActions(_retryActions, result, context);
    }

    private Task Failure(RetryResult<T> result, RetryContext context)
    {
        return InvokeActions(_failureActions, result, context);
    }

    private static async Task InvokeActions(IEnumerable<Delegate> actions, RetryResult<T> result, RetryContext context)
    {
        var clone = context.Clone();

        foreach (var action in actions)
        {
            switch (action)
            {
                case Action<RetryResult<T>, RetryContext> sync:
                    sync(result, context);
                    break;
                case Func<RetryResult<T>, RetryContext, Task> async:
                    await async(result, context);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
