using System;

namespace Shardion.Terrabreak.Services.Discord
{
    public class InvalidRequiredEntityException : ApplicationException
    {
        public InvalidRequiredEntityException() : base("A required entity was not present.")
        {
        }

        public InvalidRequiredEntityException(string message) : base(message)
        {
        }
    }
}
