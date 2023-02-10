using System;

namespace Retry.Exceptions
{
    public class SuccessCallbackException : CallbackException
    {
        /// <inheritdoc />
        public SuccessCallbackException(Exception inner) : base(
            "An exception occurs during OnSuccess execution, see InnerException", inner)
        {
        }
    }
}
