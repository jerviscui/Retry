using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Retry;

public class RetryBuilder
{
    /// <summary>
    /// You can change the DefaultOptions.
    /// </summary>
    public static readonly RetryOptions DefaultOptions = new();

    private readonly List<Type> _retryExceptions;

    private Action<RetryOptions>? _configureAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryBuilder"/> class.
    /// </summary>
    public RetryBuilder()
    {
        _retryExceptions = new List<Type>();
    }

    /// <summary>
    /// Gets the default instance.
    /// </summary>
    public static RetryBuilder Default => new();

    /// <summary>
    /// Builds this instance.
    /// </summary>
    /// <returns></returns>
    public IRetriable Build(Action @try)
    {
        return new RetryTask(@try, GetOptions(), GetRetryExceptions());
    }

    /// <summary>
    /// Builds the specified try task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="try">The try task.</param>
    /// <returns></returns>
    public IRetriable<T> Build<T>(Func<T> @try)
    {
        return new RetryTask<T>(@try, GetOptions(), GetRetryExceptions());
    }

    /// <summary>
    /// Builds this instance.
    /// </summary>
    /// <returns></returns>
    public IAsyncRetriable Build(Func<Task> @try)
    {
        return new AsyncRetryTask(@try, GetOptions(), GetRetryExceptions());
    }

    /// <summary>
    /// Builds the specified try task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="try">The try task.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> Build<T>(Func<Task<T>> @try)
    {
        return new AsyncRetryTask<T>(@try, GetOptions(), GetRetryExceptions());
    }

    private Type[] GetRetryExceptions()
    {
        return _retryExceptions.Distinct().ToArray();
    }

    private RetryOptions GetOptions()
    {
        var options = new RetryOptions
        {
            MaxTryCount = DefaultOptions.MaxTryCount,
            MaxTryTime = DefaultOptions.MaxTryTime,
            RetryInterval = DefaultOptions.RetryInterval
        };
        if (_configureAction is not null)
        {
            options = new RetryOptions();
            _configureAction(options);
        }

        if (options.MaxTryCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryOptions.MaxTryCount), "must greate or equal 1.");
        }

        return options;
    }

    /// <summary>
    /// Configures the RetryOptions.
    /// </summary>
    /// <param name="configureAction">The configure action.</param>
    /// <returns></returns>
    public RetryBuilder ConfigureOptions(Action<RetryOptions> configureAction)
    {
        _configureAction = configureAction;

        return this;
    }

    /// <summary>
    /// Retry only on exceptions of the type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <returns></returns>
    public RetryBuilder RetryOnException<TException>() where TException : Exception
    {
        return RetryOnException(typeof(TException));
    }

    /// <summary>
    /// Retry only on exceptions of the type.
    /// </summary>
    /// <param name="exceptionType">Type of the exception.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException">Parameter {nameof(exceptionType)} must be a type that is assignable to type System.Exception. - exceptionType</exception>
    public RetryBuilder RetryOnException(Type exceptionType)
    {
        if (!typeof(Exception).IsAssignableFrom(exceptionType))
        {
            throw new ArgumentException(
                $"Parameter {nameof(exceptionType)} must be a type that is assignable to type System.Exception.",
                nameof(exceptionType));
        }

        _retryExceptions.Add(exceptionType);
        return this;
    }

    /// <summary>
    /// Retries all exceptions.
    /// </summary>
    /// <returns></returns>
    public RetryBuilder RetryAllExceptions()
    {
        _retryExceptions.Clear();
        return this;
    }

    /// <summary>
    /// Configures the action to take after each time the try action fails and before the next try. 
    /// The result of the failed try action will be passed as parameter to the action.
    /// </summary>
    /// <param name="retryAction">The action to take on retry.</param>
    /// <returns></returns>
    public RetryBuilder OnRetry(Action<RetryResult> retryAction)
    {
        return this;
    }
}
