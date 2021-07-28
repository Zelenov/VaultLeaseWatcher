using System;

namespace VaultLeaseWatcher
{
    public struct Lease
    {
        public string LeaseId { get; set; }
        public bool Renewable { get; set; }
        public TimeSpan LeaseDuration { get; set; }
        public int LeaseDurationSeconds => (int) LeaseDuration.TotalSeconds;

        public string Key => LeaseId;

        public Lease(string leaseId, bool renewable, int leaseDurationSeconds) : this(leaseId, renewable,
            TimeSpan.FromSeconds(leaseDurationSeconds))
        {
        }

        public Lease(string leaseId, bool renewable, TimeSpan leaseDuration)
        {
            LeaseId = leaseId;
            Renewable = renewable;
            LeaseDuration = leaseDuration;
        }

        public override string ToString() => $"{LeaseId}";
    }
}