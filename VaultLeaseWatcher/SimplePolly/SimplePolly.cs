using System;

namespace VaultLeaseWatcher
{
    internal static class SimplePolly
    {
        public static SimplePolicy Handle(Func<Exception, bool> exceptionAssertion)
        {
            return new SimplePolicy {ExceptionAssertion = exceptionAssertion};
        }
    }
}