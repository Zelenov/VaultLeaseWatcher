using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Warnings
{
    [Serializable]
    public class LeaseRenewRelativeNotSetWarning : WarningException
    {
        public LeaseRenewRelativeNotSetWarning()
        {
        }

        public LeaseRenewRelativeNotSetWarning(string message) : base(message)
        {
        }

        public LeaseRenewRelativeNotSetWarning(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseRenewRelativeNotSetWarning(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}