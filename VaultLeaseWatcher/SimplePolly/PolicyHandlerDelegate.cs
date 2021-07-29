using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultLeaseWatcher
{
    internal delegate Task PolicyHandlerDelegate(IDictionary<string, object> context);
}