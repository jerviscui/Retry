using System;

namespace Retry.Exceptions
{
    public class RetryCallbackException : CallbackException
    {
        /// <inheritdoc />
        public RetryCallbackException(Exception inner) : base(
            "An exception occurs during OnRetry execution, see InnerException", inner)
        {
        }
    }
}
