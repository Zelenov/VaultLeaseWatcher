using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Warnings
{
    [Serializable]
    public class LeaseRenewRelativeBadValueWarning : WarningException
    {
        public LeaseRenewRelativeBadValueWarning()
        {
        }

        public LeaseRenewRelativeBadValueWarning(string message) : base(message)
        {
        }

        public LeaseRenewRelativeBadValueWarning(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseRenewRelativeBadValueWarning(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}