using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VaultLeaseWatcher.Warnings
{
    [Serializable]
    public class NonRenewableLeaseWithTtlWarning : WarningException
    {
        public NonRenewableLeaseWithTtlWarning()
        {
        }

        public NonRenewableLeaseWithTtlWarning(string message) : base(message)
        {
        }

        public NonRenewableLeaseWithTtlWarning(string message, Exception inner) : base(message, inner)
        {
        }

        protected NonRenewableLeaseWithTtlWarning(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}