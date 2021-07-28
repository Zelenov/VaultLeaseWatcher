using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Warnings
{
    [Serializable]
    public class LeaseRenewAbsoluteNotSetWarning : WarningException
    {
        public LeaseRenewAbsoluteNotSetWarning()
        {
        }

        public LeaseRenewAbsoluteNotSetWarning(string message) : base(message)
        {
        }

        public LeaseRenewAbsoluteNotSetWarning(string message, Exception inner) : base(message, inner)
        {
        }

        protected LeaseRenewAbsoluteNotSetWarning(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}