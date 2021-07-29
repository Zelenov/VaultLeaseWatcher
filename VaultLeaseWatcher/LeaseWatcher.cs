using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VaultLeaseWatcher.Exceptions;
using VaultLeaseWatcher.Options;
using VaultLeaseWatcher.Warnings;

namespace VaultLeaseWatcher
{
    public class LeaseWatcher : ILeaseWatcher, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ConcurrentDictionary<string, LeaseData>
            _leases = new ConcurrentDictionary<string, LeaseData>();

        private int _disposedCount;
        private CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposedCount, 1, 0) != 0)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            Clear();
        }

        public event LeaseWatcherLeaseEndedExceptionEvent? LeaseEnded;
        public event RenewEvent? RenewLease;
        public event LeaseWatcherRenewFailedEvent? RenewFailed;
        public event RenewStartedEvent? RenewStarted;
        public event WaitingForExpirationStartedEvent? WaitingForExpirationStarted;


        public void Stop(string leaseId)
        {
            var key = leaseId;
            if (!_leases.TryRemove(key, out var leaseData))
                return;

            leaseData.Dispose();
        }

        public void Start(LeaseWatch watch, out IList<WarningException> warnings,
            CancellationToken cancellationToken = default)
        {
            LeaseData AddValueFactory(string key)
            {
                return new LeaseData(watch.RenewLease, watch.Lease, watch.Options ?? new LeaseOptions(), watch.Tag,
                    CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, cancellationToken));
            }

            LeaseData UpdateValueFactory(string key, LeaseData existing)
            {
                existing.Dispose();
                return AddValueFactory(key);
            }

            var lease = watch.Lease;

            void RunBackgroundJob(Func<LeaseData, CancellationToken, Task> job)
            {
                var leaseData = _leases.AddOrUpdate(lease.Key, AddValueFactory, UpdateValueFactory);
                try
                {
                    var leaseData1 = leaseData;
                    Task.Run(() => job(leaseData1, leaseData1.CancellationToken), leaseData1.CancellationToken);
                }
                catch
                {
                    //oops, Task.Run failed - dispose leaseData
                    leaseData.Dispose();
                    throw;
                }
            }

            warnings = new List<WarningException>();
            var options = watch.Options ?? new LeaseOptions();
            var watchType = GetWatchType(lease, options, warnings);
            switch (watchType)
            {
                case WatchType.None: return;
                case WatchType.WaitForExpiration:
                {
                    var wait = lease.LeaseDuration;
                    RunBackgroundJob((leaseData, ct) => WaitForExpirationJobAsync(leaseData, wait, ct));
                    break;
                }
                case WatchType.Renew:
                {
                    var wait = GetNextRenewTime(options, lease.LeaseDuration, warnings);
                    RunBackgroundJob((leaseData, ct) => RenewLeaseJobAsync(leaseData, wait, ct));
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }


        private async Task RenewLeaseAsync(LeaseData leaseData)
        {
            var lease = leaseData.Lease;
            var options = leaseData.Options;
            var owner = leaseData.RenewDelegate;
            if (!_leases.TryGetValue(lease.Key, out _))
                throw new LeaseNotFoundException($"Lease {lease} not found");

            OnRenewLease(new LeaseWatcherRenewContext {Lease = lease, Options = options, Tag = leaseData.Tag});
            await owner(lease);
        }

        public void Clear()
        {
            foreach (var leaseData in _leases.Values.ToArray())
                leaseData.Dispose();
            _leases.Clear();
        }


     
        private SimplePolicy BuildPollyPolicy(LeaseOptions options)
        {
            var retryCount = options.RetryCount;
            var sleepBetweenRetries = options.SleepBetweenRetries ?? TimeSpan.Zero;
            var handleAssertion = new Func<Exception, bool>(ex => !(ex is LeaseNotFoundException));
            var policy = SimplePolly.Handle(handleAssertion).WithRetry(OnRetry);
            if (retryCount >= 0)
                policy.WithRetryCount(retryCount);

            if (sleepBetweenRetries > TimeSpan.Zero)
                policy.WithSleepDuration(sleepBetweenRetries);
            return policy;
        }

        private Task OnRetry(Exception ex, int retryCount, TimeSpan timeSpan, IDictionary<string, object> context)
        {
            var leaseData = (LeaseData) context["LeaseData"];
            OnRenewFailed(new LeaseWatcherRenewFailedContext
            {
                Exception = ex,
                TimeSpan = timeSpan,
                RetryCount = retryCount,
                Lease = leaseData.Lease,
                Options = leaseData.Options,
                Tag = leaseData.Tag
            });
            return Task.CompletedTask;
        }

        private static WatchType GetWatchType(Lease lease, LeaseOptions options, IList<WarningException> warnings)
        {
            if (lease.LeaseDuration <= TimeSpan.Zero)
                return WatchType.None;

            if (!options.AutoRenew)
            {
                warnings.Add(new AutoRenewIsOffWarning(
                    $"Lease {nameof(options.AutoRenew)} is off. Lease will expire in {lease.LeaseDuration}"));
                return WatchType.WaitForExpiration;
            }

            if (!lease.Renewable && lease.LeaseDuration >= TimeSpan.Zero)
            {
                warnings.Add(new NonRenewableLeaseWithTtlWarning(
                    $"LeaseWatcher got non renewable lease with positive ttl. Token will expire in {lease.LeaseDuration}"));
                return WatchType.WaitForExpiration;
            }

            return WatchType.Renew;
        }

        private async Task RenewLeaseJobAsync(LeaseData leaseData, TimeSpan wait, CancellationToken cancellationToken)
        {
            var lease = leaseData.Lease;
            var options = leaseData.Options;
            try
            {
                var policy = BuildPollyPolicy(options);
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        OnRenewStarted(new RenewStartedContext {Lease = lease, RenewIn = wait, Options = options});
                        await Task.Delay(wait, cancellationToken);
                        await policy.ExecuteAsync(ctx => RenewLeaseAsync((LeaseData)ctx["LeaseData"]),
                            new Dictionary<string, object> {{"LeaseData", leaseData}},
                            cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    var exception =
                        new LeaseWatcherRenewFailedException($"Fatal: vault couldn't renew lease {lease}", ex);
                    OnLeaseEnded(new LeaseWatcherLeaseEndedContext
                    {
                        Exception = exception, Lease = lease, Options = options, Tag = leaseData.Tag
                    });
                }
            }
            finally
            {
                leaseData.Dispose();
                _leases.TryRemove(lease.Key, out _);
            }
        }

        private async Task WaitForExpirationJobAsync(LeaseData leaseData, TimeSpan wait,
            CancellationToken cancellationToken)
        {
            using (leaseData)
            {
                var lease = leaseData.Lease;
                var options = leaseData.Options;
                OnWaitingForExpirationStarted(new WaitingForExpirationStartedContext
                {
                    Lease = lease, ExpireIn = wait, Options = options, Tag = leaseData.Tag
                });
                await Task.Delay(wait, cancellationToken);
                try
                {
                    if (!_leases.TryRemove(lease.Key, out _))
                        return;

                    throw new LeaseWatcherLeaseEndedException($"Lease {lease.LeaseId} ended");
                }
                catch (Exception ex)
                {
                    OnLeaseEnded(new LeaseWatcherLeaseEndedContext
                    {
                        Exception = ex, Lease = lease, Options = options, Tag = leaseData.Tag
                    });
                }
            }
        }

        private static double GetRelativeRenew(LeaseOptions options, IList<WarningException> warnings)
        {
            const double defaultRelativeRenew = 0.5;
            if (options.LeaseRenewRelative == null)
            {
                warnings.Add(new LeaseRenewRelativeNotSetWarning(
                    $"{nameof(options.LeaseRenewRelative)} not set for {nameof(LeaseRenewPolicy.Relative)} renew policy. Assuming it equals to {defaultRelativeRenew}"));
                return defaultRelativeRenew;
            }

            if (options.LeaseRenewRelative.Value < 0 || options.LeaseRenewRelative.Value > 1)
            {
                warnings.Add(new LeaseRenewRelativeBadValueWarning(
                    $"{nameof(options.LeaseRenewRelative)} doesn't fit in [0..1] interval. Assuming it equals to {defaultRelativeRenew}"));
                return defaultRelativeRenew;
            }

            return options.LeaseRenewRelative.Value;
        }

        private TimeSpan GetNextRenewTime(LeaseOptions options, TimeSpan leaseDuration,
            IList<WarningException> warnings)
        {
            var renewPolicy = GetLeaseRenewPolicy(options, leaseDuration);
            switch (renewPolicy)
            {
                case LeaseRenewPolicy.Relative:
                {
                    var relativeRenew = GetRelativeRenew(options, warnings);
                    return TimeSpan.FromTicks((long) (leaseDuration.Ticks * relativeRenew));
                }
                case LeaseRenewPolicy.Absolute:
                    if (options.LeaseRenewAbsolute == null)
                    {
                        var relativeRenew = GetRelativeRenew(options, warnings);
                        warnings.Add(new LeaseRenewAbsoluteNotSetWarning(
                            $"{nameof(options.LeaseRenewAbsolute)} not set for {nameof(LeaseRenewPolicy.Absolute)} renew policy. Assuming renew is set to {nameof(LeaseRenewPolicy.Relative)} with value {relativeRenew}"));
                        return TimeSpan.FromTicks((long) (leaseDuration.Ticks * relativeRenew));
                    }

                    if (options.LeaseRenewAbsolute >= leaseDuration)
                        warnings.Add(new LeaseRenewAbsoluteIsGreaterThanTtlWarning(
                            $"{nameof(options.LeaseRenewAbsolute)} > ttl. Token might be revoked by that time"));

                    return options.LeaseRenewAbsolute.Value;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(LeaseRenewPolicy)} {renewPolicy} not supported");
            }
        }

        private static LeaseRenewPolicy GetLeaseRenewPolicy(LeaseOptions options, TimeSpan leaseDuration)
        {
            if (options.LeaseRenewAbsolute == null)
                return LeaseRenewPolicy.Relative;

            if (options.LeaseRenewRelative == null || leaseDuration >= options.LeaseRenewAbsolute)
                return LeaseRenewPolicy.Absolute;

            return LeaseRenewPolicy.Relative;
        }

        protected virtual void OnRenewLease(LeaseWatcherRenewContext context)
        {
            RenewLease?.Invoke(this, context);
        }

        protected virtual void OnLeaseEnded(LeaseWatcherLeaseEndedContext context)
        {
            LeaseEnded?.Invoke(this, context);
        }


        protected virtual void OnRenewFailed(LeaseWatcherRenewFailedContext context)
        {
            RenewFailed?.Invoke(this, context);
        }

        protected virtual void OnRenewStarted(RenewStartedContext context)
        {
            RenewStarted?.Invoke(this, context);
        }

        protected virtual void OnWaitingForExpirationStarted(WaitingForExpirationStartedContext context)
        {
            WaitingForExpirationStarted?.Invoke(this, context);
        }
    }
}