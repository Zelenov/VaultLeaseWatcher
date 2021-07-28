using System;

namespace VaultLeaseWatcher
{
    public class LeaseWatcherRenewFailedContext
    {
        public Lease Lease { get; set; }
        public LeaseOptions Options { get; set; } = null!;
        public object? Tag { get; set; }
        public Exception Exception { get; set; } = null!;
        public TimeSpan TimeSpan { get; set; }
        public int RetryCount { get; set; }
    }
}