using System;

namespace Retry;

public struct RetryContext
{
    public RetryContext(int triedCount, int retryCount, TimeSpan triedTime)
    {
        TriedCount = triedCount;
        RetryCount = retryCount;
        TriedTime = triedTime;
    }

    /// <summary>
    /// The tried count
    /// </summary>
    public int TriedCount;

    /// <summary>
    /// The retry count
    /// </summary>
    public int RetryCount;

    /// <summary>
    /// The tried time
    /// </summary>
    public TimeSpan TriedTime;

    public RetryContext Clone() => new(TriedCount, RetryCount, TriedTime);
}
