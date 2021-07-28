using System;

namespace VaultLeaseWatcher
{
    public class LeaseOptions
    {
        public int RetryCount { get; set; } = 0;
        public TimeSpan? SleepBetweenRetries { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan? LeaseRenewAbsolute { get; set; } = null;
        public double? LeaseRenewRelative { get; set; } = 0.5;
        public bool AutoRenew { get; set; } = true;
    }
}