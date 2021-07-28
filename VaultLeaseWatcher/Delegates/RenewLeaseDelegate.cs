using System.Threading.Tasks;

namespace VaultLeaseWatcher
{
    public delegate Task RenewLeaseDelegate(Lease lease);
}