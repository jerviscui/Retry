using System;

namespace Retry;

public class RetryOptions
{
    /// <summary>
    /// The time interval to wait between each retry attempt.
    /// </summary>
    public TimeSpan TryInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// The max try time limit.
    /// </summary>
    public TimeSpan MaxTryTime { get; set; } = TimeSpan.MaxValue;

    /// <summary>
    /// The max try count limit.
    /// </summary>
    public int MaxTryCount { get; set; } = 2;
}
