using System;

namespace VaultLeaseWatcher
{
    public class RenewStartedContext
    {
        public Lease Lease { get; set; }
        public LeaseOptions Options { get; set; } = null!;
        public object? Tag { get; set; }
        public TimeSpan RenewIn { get; set; }
    }
}