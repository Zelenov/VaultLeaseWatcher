using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Warnings
{
    [Serializable]
    public class AutoRenewIsOffWarning : WarningException
    {
        public AutoRenewIsOffWarning()
        {
        }

        public AutoRenewIsOffWarning(string message) : base(message)
        {
        }

        public AutoRenewIsOffWarning(string message, Exception inner) : base(message, inner)
        {
        }

        protected AutoRenewIsOffWarning(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}