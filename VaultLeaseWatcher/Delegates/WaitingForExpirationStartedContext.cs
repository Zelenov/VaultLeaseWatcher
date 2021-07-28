using System;

namespace VaultLeaseWatcher
{
    public class WaitingForExpirationStartedContext
    {
        public Lease Lease { get; set; }
        public LeaseOptions Options { get; set; } = null!;
        public object? Tag { get; set; }
        public TimeSpan ExpireIn { get; set; }
    }
}