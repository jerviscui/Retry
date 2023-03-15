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

    private TimeSpan _initial;

    private TimeSpan _maxDelay;

    private TimeSpan _pre;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExponentialRetryInterval"/> class.
    /// </summary>
    /// <param name="initial">The initial.</param>
    /// <param name="max">The maximum.</param>
    public ExponentialRetryInterval(TimeSpan initial, TimeSpan? max = null)
    {
        _initial = initial;
        _pre = initial;
        _maxDelay = max ?? TimeSpan.MaxValue;

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
        var delay = Math.Min(_initial.TotalMilliseconds * delta, _maxDelay.TotalMilliseconds);

        _pre = TimeSpan.FromMilliseconds(delay);

        return _pre;
    }
}
