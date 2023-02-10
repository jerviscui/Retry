using System;

namespace Retry.Exceptions
{
    public class FailureCallbackException : CallbackException
    {
        /// <inheritdoc />
        public FailureCallbackException(Exception inner) : base(
            "An exception occurs during OnFailure execution, see InnerException", inner)
        {
        }
    }
}
