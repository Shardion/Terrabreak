using System;

namespace Shardion.Terrabreak.Services.Options
{
    public class NoAccessException : ApplicationException
    {
        public NoAccessException()
        {
        }

        public NoAccessException(string? message) : base(message)
        {
        }

        public NoAccessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
