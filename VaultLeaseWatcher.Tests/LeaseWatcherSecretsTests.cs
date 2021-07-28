using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace VaultLeaseWatcher.Tests
{
    [TestFixture]
    public class LeaseWatcherSecretsTests
    {
        protected LeaseWatcher LeaseWatcher => new LeaseWatcher();

        [Test]
        public void Run_InstantSuccess_Success()
        {
            var handicap = TimeSpan.FromSeconds(1);
            var leaseRenewAbsolute = TimeSpan.FromSeconds(2);
            var sleepBetweenRetries = TimeSpan.FromSeconds(2);

            Task RenewLeaseFunc(Lease _)
            {
                return Task.CompletedTask;
            }

            var lease = new Lease("leaseId", true, TimeSpan.FromHours(1));
            LeaseOptions options = new LeaseOptions
            {
                AutoRenew = true, LeaseRenewAbsolute = leaseRenewAbsolute, SleepBetweenRetries = sleepBetweenRetries
            };
            var watch = new LeaseWatch {RenewLease = RenewLeaseFunc, Lease = lease, Options = options};

            using var leaseWatcher = LeaseWatcher;
            var renews = new List<LeaseWatcherRenewContext>();
            leaseWatcher.RenewLease += (sender, ctx) => renews.Add(ctx);
            leaseWatcher.Start(watch, out var warnings);
            CollectionAssert.IsEmpty(warnings);
            var wait = leaseRenewAbsolute.TotalMilliseconds + handicap.TotalMilliseconds;
            Assert.That(() => renews.Count, Is.EqualTo(1).After((int) wait, 100), "Token was not renewed");
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Run_FailsNTimesThenSucceed_Success(int failCount)
        {
            var handicap = TimeSpan.FromSeconds(1);
            var leaseRenewAbsolute = TimeSpan.FromSeconds(2);
            var sleepBetweenRetries = TimeSpan.FromSeconds(2);
            var maxFailCount = failCount + 1;
            var renews = new List<LeaseWatcherRenewContext>();
            var fails = new List<LeaseWatcherRenewFailedContext>();

            Task RenewLeaseFunc(Lease _)
            {
                return renews.Count > failCount ? Task.CompletedTask : throw new Exception("test exception");
            }

            var lease = new Lease("leaseId", true, TimeSpan.FromHours(1));
            LeaseOptions options = new LeaseOptions
            {
                AutoRenew = true,
                LeaseRenewAbsolute = leaseRenewAbsolute,
                SleepBetweenRetries = sleepBetweenRetries,
                RetryCount = maxFailCount
            };
            var watch = new LeaseWatch {RenewLease = RenewLeaseFunc, Lease = lease, Options = options};

            using var leaseWatcher = LeaseWatcher;
            leaseWatcher.RenewLease += (sender, ctx) => renews.Add(ctx);
            leaseWatcher.RenewFailed += (sender, ctx) => fails.Add(ctx);
            leaseWatcher.Start(watch, out var warnings);
            CollectionAssert.IsEmpty(warnings);
            var wait = (sleepBetweenRetries + leaseRenewAbsolute).TotalMilliseconds * failCount +
                handicap.TotalMilliseconds;
            Assert.That(() => fails.Count, Is.EqualTo(failCount).After((int) wait, 100), "Token renew didn't fail");
            wait = (sleepBetweenRetries + leaseRenewAbsolute + handicap).TotalMilliseconds;
            Assert.That(() => renews.Count, Is.EqualTo(failCount + 1).After((int) wait, 100), "Token was not renewed");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Add_FailsEveryTime_ThrowsRenewEnded(int failCount)
        {
            var handicap = TimeSpan.FromSeconds(1);
            var leaseRenewAbsolute = TimeSpan.FromSeconds(2);
            var sleepBetweenRetries = TimeSpan.FromSeconds(2);
            var maxFailCount = failCount;

            var renews = new List<LeaseWatcherRenewContext>();
            var fails = new List<LeaseWatcherRenewFailedContext>();
            var ended = new List<LeaseWatcherLeaseEndedContext>();

            Task RenewLeaseFunc(Lease _)
            {
                throw new Exception("test exception");
            }

            var lease = new Lease("leaseId", true, TimeSpan.FromHours(1));
            LeaseOptions options = new LeaseOptions
            {
                AutoRenew = true,
                LeaseRenewAbsolute = leaseRenewAbsolute,
                SleepBetweenRetries = sleepBetweenRetries,
                RetryCount = maxFailCount
            };
            var watch = new LeaseWatch {RenewLease = RenewLeaseFunc, Lease = lease, Options = options};

            using var leaseWatcher = LeaseWatcher;

            leaseWatcher.RenewLease += (sender, ctx) => renews.Add(ctx);
            leaseWatcher.RenewFailed += (sender, ctx) => fails.Add(ctx);
            leaseWatcher.LeaseEnded += (sender, ctx) => ended.Add(ctx);

            leaseWatcher.Start(watch, out var warnings);

            CollectionAssert.IsEmpty(warnings);
            var wait = (sleepBetweenRetries + leaseRenewAbsolute).TotalMilliseconds * failCount +
                handicap.TotalMilliseconds;
            Assert.That(() => ended.Count, Is.EqualTo(1).After((int) wait, 100), "Token renew didn't fail");

            Assert.AreEqual(failCount + 1, renews.Count);
            Assert.AreEqual(1, ended.Count);
            Assert.AreEqual(failCount, fails.Count);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Remove_FailsEveryTime_SucessfullyStops(int failCount)
        {
            var handicap = TimeSpan.FromSeconds(1);
            var leaseRenewAbsolute = TimeSpan.FromSeconds(2);
            var sleepBetweenRetries = TimeSpan.FromSeconds(2);
            var maxFailCount = failCount + 1;

            var renews = new List<LeaseWatcherRenewContext>();
            var fails = new List<LeaseWatcherRenewFailedContext>();
            var ended = new List<LeaseWatcherLeaseEndedContext>();

            Task RenewLeaseFunc(Lease _)
            {
                throw new Exception("test exception");
            }

            var lease = new Lease("leaseId", true, TimeSpan.FromHours(1));
            LeaseOptions options = new LeaseOptions
            {
                AutoRenew = true,
                LeaseRenewAbsolute = leaseRenewAbsolute,
                SleepBetweenRetries = sleepBetweenRetries,
                RetryCount = maxFailCount
            };
            var watch = new LeaseWatch {RenewLease = RenewLeaseFunc, Lease = lease, Options = options};

            using var leaseWatcher = LeaseWatcher;

            leaseWatcher.RenewLease += (sender, ctx) => renews.Add(ctx);
            leaseWatcher.RenewFailed += (sender, ctx) => fails.Add(ctx);
            leaseWatcher.LeaseEnded += (sender, ctx) => ended.Add(ctx);

            leaseWatcher.Start(watch, out var warnings);

            CollectionAssert.IsEmpty(warnings);

            var wait = (sleepBetweenRetries + leaseRenewAbsolute).TotalMilliseconds * failCount +
                handicap.TotalMilliseconds;
            Assert.That(() => fails.Count, Is.EqualTo(failCount).After((int) wait, 100), "Token renew didn't fail");
            wait = (sleepBetweenRetries + leaseRenewAbsolute + handicap).TotalMilliseconds;

            leaseWatcher.Stop(lease.LeaseId);

            Assert.That(() => renews.Count, Is.EqualTo(failCount).After((int) wait, 100),
                "Token was renewed after stp");
        }
    }
}