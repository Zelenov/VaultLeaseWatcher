namespace VaultLeaseWatcher
{
    public class LeaseWatcherRenewContext
    {
        public Lease Lease { get; set; }
        public LeaseOptions Options { get; set; } = null!;
        public object? Tag { get; set; }
    }
}