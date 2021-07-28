using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Warnings
{
    [Serializable]
    public class LeaseRenewAbsoluteIsGreaterThanTtlWarning : WarningException
    {
        public LeaseRenewAbsoluteIsGreaterThanTtlWarning()
        {
        }

        public LeaseRenewAbsoluteIsGreaterThanTtlWarning(string message) : base(message)
        {
        }

        public LeaseRenewAbsoluteIsGreaterThanTtlWarning(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseRenewAbsoluteIsGreaterThanTtlWarning(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }
    }
}