using System;

namespace Retry.Exceptions
{
    public class OverMaxTryCountException : Exception
    {
        /// <summary>
        /// Total execution count.
        /// </summary>
        public int TriedCount { get; }

        /// <inheritdoc />
        public OverMaxTryCountException(int triedCount)
        {
            TriedCount = triedCount;
        }
    }
}
