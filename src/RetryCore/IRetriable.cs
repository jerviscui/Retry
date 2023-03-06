using System;

namespace Retry;

public interface IRetriable
{
    /// <summary>
    /// Try action as an asynchronous method.
    /// </summary>
    /// <returns></returns>
    public IAsyncRetriable AsAsync();

    /// <summary>
    /// Execution try action.
    /// </summary>
    /// <returns></returns>
    public RetryResult Run();

    /// <summary>
    /// Execution try action.
    /// Retries the task until the specified condition return true.
    /// </summary>
    /// <param name = "condition">The assert condition.</param>
    /// <returns></returns>
    public RetryResult Run(Func<RetryResult, bool> condition);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action will be passed as parameter to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IRetriable OnRetry(Action<RetryResult> retryAction);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IRetriable OnRetry(Action<RetryResult, RetryContext> retryAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IRetriable OnSuccess(Action<RetryResult> successAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt and the total count of attempts 
    /// are passed as parameters to the action. This count includes the 
    /// final successful one.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IRetriable OnSuccess(Action<RetryResult, RetryContext> successAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed. 
    /// The result of the last failed attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IRetriable OnFailure(Action<RetryResult> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt and the total count of attempts 
    /// are passed as parameters to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IRetriable OnFailure(Action<RetryResult, RetryContext> failureAction);
}

public interface IRetriable<T>
{
    /// <summary>
    /// Try action as an asynchronous method.
    /// </summary>
    /// <returns></returns>
    public IAsyncRetriable<T> AsAsync();

    /// <summary>
    /// Execution try action.
    /// </summary>
    /// <returns></returns>
    public RetryResult<T> Run();

    /// <summary>
    /// Execution try action.
    /// Retries the task until the specified condition return true.
    /// </summary>
    /// <param name = "condition">The assert condition.</param>
    /// <returns></returns>
    public RetryResult<T> Run(Func<RetryResult<T>, bool> condition);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action will be passed as parameter to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IRetriable<T> OnRetry(Action<RetryResult<T>> retryAction);

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action and the total count of attempts that 
    /// have been performed are passed as parameters to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public IRetriable<T> OnRetry(Action<RetryResult<T>, RetryContext> retryAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IRetriable<T> OnSuccess(Action<RetryResult<T>> successAction);

    /// <summary>
    /// Configures the action to take when the try action succeeds.
    /// The result of the successful attempt and the total count of attempts 
    /// are passed as parameters to the action. This count includes the 
    /// final successful one.
    /// </summary>
    /// <param name="successAction">The action to take on success.</param>
    /// <returns></returns>
    public IRetriable<T> OnSuccess(Action<RetryResult<T>, RetryContext> successAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed. 
    /// The result of the last failed attempt is passed as parameter to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IRetriable<T> OnFailure(Action<RetryResult<T>> failureAction);

    /// <summary>
    /// Configures the action to take when the try action execution failed.
    /// The result of the last failed attempt and the total count of attempts 
    /// are passed as parameters to the action.
    /// </summary>
    /// <param name="failureAction">The action to take on failure.</param>
    /// <returns></returns>
    public IRetriable<T> OnFailure(Action<RetryResult<T>, RetryContext> failureAction);
}
