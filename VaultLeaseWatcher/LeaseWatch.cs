namespace VaultLeaseWatcher
{
    public class LeaseWatch
    {
        public RenewLeaseDelegate RenewLease { get; set; } = null!;
        public Lease Lease { get; set; }
        public LeaseOptions? Options { get; set; }
        public object? Tag { get; set; }
    }
}