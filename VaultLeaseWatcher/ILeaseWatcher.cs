using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace VaultLeaseWatcher
{
    public interface ILeaseWatcher
    {
        void Stop(string leaseId);

        void Start(LeaseWatch watch, out IList<WarningException> warnings,
            CancellationToken cancellationToken = default);

        event LeaseWatcherLeaseEndedExceptionEvent? LeaseEnded;
        event RenewEvent? RenewLease;
        event LeaseWatcherRenewFailedEvent? RenewFailed;
        event RenewStartedEvent? RenewStarted;
        event WaitingForExpirationStartedEvent? WaitingForExpirationStarted;
    }
}