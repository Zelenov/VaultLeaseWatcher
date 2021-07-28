using System;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Exceptions
{
    [Serializable]
    public class LeaseWatcherRenewFailedException : Exception
    {
        public LeaseWatcherRenewFailedException()
        {
        }

        public LeaseWatcherRenewFailedException(string message) : base(message)
        {
        }

        public LeaseWatcherRenewFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseWatcherRenewFailedException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}