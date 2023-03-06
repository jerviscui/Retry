using System;

namespace Retry;

/// <summary>
/// Time grows exponentially based on the number of executions.
/// </summary>
/// <seealso cref="Retry.IRetryIntervalStrategy" />
public class ExponentialRetryInterval : IRetryIntervalStrategy
{
    private readonly Random _random;

    private int _count;

    private TimeSpan _first;

    private TimeSpan _maxDelay;

    private TimeSpan _pre;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExponentialRetryInterval"/> class.
    /// </summary>
    /// <param name="first">The first.</param>
    /// <param name="max">The maximum.</param>
    public ExponentialRetryInterval(TimeSpan first, TimeSpan max)
    {
        _first = first;
        _pre = first;
        _maxDelay = max;

        _random = new Random();
    }

    /// <inheritdoc />
    public TimeSpan GetInterval()
    {
        _count++;

        if (_pre >= _maxDelay)
        {
            return _pre;
        }

        var delta = (Math.Pow(2, _count) - 1.0) * (1.0 + (_random.NextDouble() * (1.1 - 1.0)));
        var delay = Math.Min(_first.TotalMilliseconds * delta, _maxDelay.TotalMilliseconds);

        _pre = TimeSpan.FromMilliseconds(delay);

        return _pre;
    }
}
