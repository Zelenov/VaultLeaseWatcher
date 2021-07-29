using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultLeaseWatcher
{
    internal delegate Task PolicyRetryDelegate(Exception ex, int retryCount, TimeSpan timeSpan, IDictionary<string, object> context);
}