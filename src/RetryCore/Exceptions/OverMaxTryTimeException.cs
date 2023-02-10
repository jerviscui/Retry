using System;

namespace Retry.Exceptions
{
    public class OverMaxTryTimeException : Exception
    {
        /// <summary>
        /// Total execution time.
        /// </summary>
        public TimeSpan TriedTime { get; }

        /// <inheritdoc />
        public OverMaxTryTimeException(TimeSpan triedTime)
        {
            TriedTime = triedTime;
        }
    }
}
