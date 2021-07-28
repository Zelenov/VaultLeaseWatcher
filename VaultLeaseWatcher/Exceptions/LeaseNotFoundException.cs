using System;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Exceptions
{
    [Serializable]
    public class LeaseNotFoundException : Exception
    {
        public LeaseNotFoundException()
        {
        }

        public LeaseNotFoundException(string message) : base(message)
        {
        }

        public LeaseNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}