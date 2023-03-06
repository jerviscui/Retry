using System;

namespace Retry;

public interface IRetryIntervalStrategy
{
    /// <summary>
    /// Gets the next interval time.
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetInterval();
}
