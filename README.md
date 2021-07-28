# VaultLeaseWatcher
Lease expiration watcher for [VaultSharp client](https://github.com/rajanadar/VaultSharp). Renews secrets' leases on time, retries multiple times on fail, gives all logging possibilities via events and checks lease expiration for non-renewable secrets;

# Getting started
```csharp
using var leaseWatcher = new LeaseWatcher(); //don't forget to dispose a client
leaseWatcher.RenewLease += (sender, ctx) => {}; // runs before every renew
leaseWatcher.RenewFailed += (sender, ctx) => {}; // runs after failed renew attempt
leaseWatcher.LeaseEnded += (sender, ctx) => {}; // runs after all attempts failed

//get lease using VaultSharp
Secret<UsernamePasswordCredentials> dbCreds = await vaultClient.V1.Secrets.Database.GetCredentialsAsync(role);
string username = dbCreds.Data.Username;
string password = dbCreds.Data.Password;

//convert it into a watch
Lease lease = new Lease(dbCreds.LeaseId, renewable: dbCreds.Renewable, leaseDuration: dbCreds.LeaseDurationSeconds);
LeaseOptions options = new LeaseOptions
{
    LeaseRenewAbsolute = TimeSpan.FromHours(1), //renew lease every hour
    LeaseRenewRelative = 0.5, //renew lease after half of ttl has passed
    SleepBetweenRetries = TimeSpan.FromSeconds(10), //wait 10 seconds beetween failed renew tries
    RetryCount = 10 //try to renew lease 10 times before failing
};
var renewLeaseFunc = lease => _client.V1.System.RenewLeaseAsync(lease.LeaseId, lease.LeaseDurationSeconds);
var watch = new LeaseWatch {RenewLease = renewLeaseFunc, Lease = lease, Options = options};

//start watch
watcher.Start(watch, out IList<WarningException>? warnings);
//log warnings

..
//stop watching lease anytime 
watcher.Stop(dbCreds.LeaseId); 
```
