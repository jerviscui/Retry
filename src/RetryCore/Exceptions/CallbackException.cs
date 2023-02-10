using System;

namespace Retry.Exceptions
{
    public abstract class CallbackException : Exception
    {
        /// <inheritdoc />
        protected CallbackException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}