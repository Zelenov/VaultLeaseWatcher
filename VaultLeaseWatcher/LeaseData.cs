using System;
using System.Threading;

namespace VaultLeaseWatcher
{
    internal struct LeaseData : IDisposable
    {
        private int _disposedCount;
        public Lease Lease { get; set; }
        public LeaseOptions Options { get; set; }
        public RenewLeaseDelegate RenewDelegate { get; set; }
        public object? Tag { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public LeaseData(RenewLeaseDelegate renewDelegate, Lease lease, LeaseOptions options, object? tag,
            CancellationTokenSource cancellationTokenSource)
        {
            _disposedCount = 0;
            RenewDelegate = renewDelegate;
            Lease = lease;
            Options = options;
            Tag = tag;
            CancellationTokenSource = cancellationTokenSource;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposedCount, 1, 0) != 0)
                return;

            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }
    }
}