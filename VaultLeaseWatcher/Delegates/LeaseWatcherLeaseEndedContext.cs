using System;

namespace VaultLeaseWatcher
{
    public class LeaseWatcherLeaseEndedContext
    {
        public Lease Lease { get; set; }
        public LeaseOptions Options { get; set; } = null!;
        public Exception Exception { get; set; } = null!;
        public object? Tag { get; set; }
    }
}