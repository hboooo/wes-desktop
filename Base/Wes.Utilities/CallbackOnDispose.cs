using System;
using System.Threading;

namespace Wes.Utilities
{
    public sealed class CallbackOnDispose : IDisposable
    {
        Action action;

        public CallbackOnDispose(Action action)
        {
            this.action = action ?? throw new ArgumentNullException("action");
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref action, null)?.Invoke();
        }
    }
}
