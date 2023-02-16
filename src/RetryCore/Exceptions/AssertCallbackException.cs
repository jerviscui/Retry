using System;

namespace Retry.Exceptions
{
    public class AssertCallbackException : CallbackException
    {
        /// <inheritdoc />
        public AssertCallbackException(Exception inner) : base(
            "An exception occurs during Assert execution, see InnerException", inner)
        {
        }
    }
}
