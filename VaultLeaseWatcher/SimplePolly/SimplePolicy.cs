using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VaultLeaseWatcher
{
    internal class SimplePolicy
    {
        public Func<Exception, bool>? ExceptionAssertion { get; set; }
        public TimeSpan? SleepDuration { get; set; }
        public int? RetryCount { get; set; }
        public PolicyRetryDelegate? PolicyRetryDelegate { get; set; }

        public SimplePolicy WithRetry(PolicyRetryDelegate policyRetryDelegate)
        {
            PolicyRetryDelegate = policyRetryDelegate;
            return this;
        }
        
        public SimplePolicy WithRetryCount(int retryCount)
        {
            RetryCount = retryCount;
            return this;
        }

        public SimplePolicy WithSleepDuration(TimeSpan sleepDuration)
        {
            SleepDuration = sleepDuration;
            return this;
        }

        public async Task ExecuteAsync(PolicyHandlerDelegate policyDelegate, IDictionary<string, object> context,
            CancellationToken cancellationToken)
        {
            int retryCount = 0;
            TimeSpan timeSpan = SleepDuration ?? TimeSpan.Zero;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (RetryCount != null)
                    retryCount++;
                try
                {
                    await policyDelegate(context);
                    return;
                }
                catch (Exception ex)
                {
                    if (ExceptionAssertion == null || !ExceptionAssertion(ex))
                        throw;

                    if (RetryCount != null && retryCount > RetryCount.Value)
                        throw;

                    if (PolicyRetryDelegate != null)
                        await PolicyRetryDelegate(ex, retryCount, timeSpan, context);
                    if (SleepDuration != null)
                        await Task.Delay(SleepDuration.Value, cancellationToken);
                }
            }
        }
    }
}