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
    public IRetriable Build(Action tryTask)
    {
        return new RetryTask(tryTask, GetOptions(), GetRetryExceptions());
    }

    /// <summary>
    /// Builds the specified try task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tryTask">The try task.</param>
    /// <returns></returns>
    public IRetriable<T> Build<T>(Func<T> tryTask)
    {
        return new RetryTask<T>(tryTask, GetOptions(), GetRetryExceptions());
    }

    /// <summary>
    /// Builds this instance.
    /// </summary>
    /// <returns></returns>
    public IAsyncRetriable Build(Func<Task> tryTask)
    {
        return new AsyncRetryTask(tryTask, GetOptions(), GetRetryExceptions());
    }

    /// <summary>
    /// Builds the specified try task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tryTask">The try task.</param>
    /// <returns></returns>
    public IAsyncRetriable<T> Build<T>(Func<Task<T>> tryTask)
    {
        return new AsyncRetryTask<T>(tryTask, GetOptions(), GetRetryExceptions());
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
            TryInterval = DefaultOptions.TryInterval
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
}
