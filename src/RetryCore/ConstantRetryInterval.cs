using System;

namespace Retry;

/// <summary>
/// A constant time.
/// </summary>
/// <seealso cref="Retry.IRetryIntervalStrategy" />
public class ConstantRetryInterval : IRetryIntervalStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantRetryInterval"/> class.
    /// </summary>
    /// <param name="interval">The interval.</param>
    public ConstantRetryInterval(TimeSpan interval) => _interval = interval;

    private readonly TimeSpan _interval;

    /// <inheritdoc />
    public TimeSpan GetInterval() => _interval;
}
