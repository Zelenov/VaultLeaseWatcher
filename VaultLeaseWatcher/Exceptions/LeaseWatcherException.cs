using System;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Exceptions
{
    [Serializable]
    public class LeaseWatcherException : Exception
    {
        public LeaseWatcherException()
        {
        }

        public LeaseWatcherException(string message) : base(message)
        {
        }

        public LeaseWatcherException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseWatcherException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}