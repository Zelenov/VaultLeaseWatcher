using System;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Exceptions
{
    [Serializable]
    public class LeaseWatcherLeaseEndedException : Exception
    {
        public LeaseWatcherLeaseEndedException()
        {
        }

        public LeaseWatcherLeaseEndedException(string message) : base(message)
        {
        }

        public LeaseWatcherLeaseEndedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseWatcherLeaseEndedException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}