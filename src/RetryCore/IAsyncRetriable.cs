using System;
using System.Threading;
using System.Threading.Tasks;

namespace Retry;

public interface IAsyncRetriable
{
    /// <summary>
    /// Asynchronous execution try action.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
    /// <param name="continueOnCapturedContext">
    /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
    /// </param>
    /// <returns></returns>
    public Task<RetryResult> RunAsync(CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false);

    /// <summary>
    /// Asynchronous execution try action.
    /// Retries the task until the specified condition return true.
    /// </summary>
    /// <param name = "condition">The assert condition.</param>
    /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
    /// <param name="continueOnCapturedContext">
    /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
    /// </param>
    /// <returns></returns>
    public Task<RetryResult> RunAsync(Func<RetryResult, bool> condition, CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false);

    /// <summary>
    /// Asynchronous execution try action.
    /// Retries the task until the specified condition return true.
    /// </summary>
    /// <param name = "condition">The assert condition.</param>
    /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
    /// <param name="continueOnCapturedContext">
    /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
    /// </param>
    /// <returns></returns>
    public Task<RetryResult> RunAsync(Func<RetryResult, Task<bool>> condition,
        CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action will be passed as parameter to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable OnRetry(Action<RetryResult> retryAction);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable OnRetry(Action<RetryResult, RetryContext> retryAction);

    /// <summary>
    /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable OnRetryAsync(Func<RetryResult, Task> retryAction);

    /// <summary>
    /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable OnRetryAsync(Func<RetryResult, RetryContext, Task> retryAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable OnSuccess(Action<RetryResult> successAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt and the total count of attempts 
    /// are passed as parameters to the action. This count includes the 
    /// final successful one.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable OnSuccess(Action<RetryResult, RetryContext> successAction);

    /// <summary>
    /// Configures the asynchronous action to take when the try action succeeds.
    /// The result of the successful attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable OnSuccessAsync(Func<RetryResult, Task> successAction);

    /// <summary>
    /// Configures the asynchronous action to take when the try action succeeds.
    /// The result of the successful attempt and the total count of attempts 
    /// are passed as parameters to the action. This count includes the 
    /// final successful one.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable OnSuccessAsync(Func<RetryResult, RetryContext, Task> successAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed. 
    /// The result of the last failed attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable OnFailure(Action<RetryResult> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt and the total count of attempts 
    /// are passed as parameters to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable OnFailure(Action<RetryResult, RetryContext> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable OnFailureAsync(Func<RetryResult, Task> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt and the total count of attempts 
    /// are passed as parameters to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable OnFailureAsync(Func<RetryResult, RetryContext, Task> failureAction);
}

public interface IAsyncRetriable<T>
{
    /// <summary>
    /// Asynchronous execution try action.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
    /// <param name="continueOnCapturedContext">
    /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
    /// </param>
    /// <returns></returns>
    public Task<RetryResult<T>> RunAsync(CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false);

    /// <summary>
    /// Asynchronous execution try action.
    /// Retries the task until the specified condition return true.
    /// </summary>
    /// <param name = "condition">The assert condition.</param>
    /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
    /// <param name="continueOnCapturedContext">
    /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
    /// </param>
    /// <returns></returns>
    public Task<RetryResult<T>> RunAsync(Func<RetryResult<T>, bool> condition,
        CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false);

    /// <summary>
    /// Asynchronous execution try action.
    /// Retries the task until the specified condition return true.
    /// </summary>
    /// <param name = "condition">The assert condition.</param>
    /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
    /// <param name="continueOnCapturedContext">
    /// true to attempt to marshal the continuation back to the original context captured; otherwise, false.
    /// </param>
    /// <returns></returns>
    public Task<RetryResult<T>> RunAsync(Func<RetryResult<T>, Task<bool>> condition,
        CancellationToken cancellationToken = default,
        bool continueOnCapturedContext = false);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action will be passed as parameter to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnRetry(Action<RetryResult<T>> retryAction);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnRetry(Action<RetryResult<T>, RetryContext> retryAction);

    /// <summary>
    /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnRetryAsync(Func<RetryResult<T>, Task> retryAction);

    /// <summary>
    /// Configures the asynchronous action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnRetryAsync(Func<RetryResult<T>, RetryContext, Task> retryAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnSuccess(Action<RetryResult<T>> successAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt and the total count of attempts 
    /// are passed as parameters to the action. This count includes the 
    /// final successful one.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnSuccess(Action<RetryResult<T>, RetryContext> successAction);

    /// <summary>
    /// Configures the asynchronous action to take when the try action succeeds.
    /// The result of the successful attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnSuccessAsync(Func<RetryResult<T>, Task> successAction);

    /// <summary>
    /// Configures the asynchronous action to take when the try action succeeds.
    /// The result of the successful attempt and the total count of attempts 
    /// are passed as parameters to the action. This count includes the 
    /// final successful one.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnSuccessAsync(Func<RetryResult<T>, RetryContext, Task> successAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed. 
    /// The result of the last failed attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnFailure(Action<RetryResult<T>> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt and the total count of attempts 
    /// are passed as parameters to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnFailure(Action<RetryResult<T>, RetryContext> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnFailureAsync(Func<RetryResult<T>, Task> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt and the total count of attempts 
    /// are passed as parameters to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> OnFailureAsync(Func<RetryResult<T>, RetryContext, Task> failureAction);
}
