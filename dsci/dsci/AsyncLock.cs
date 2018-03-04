using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsci
{
    public class AsyncLock
    {
        private readonly Queue<TaskCompletionSource<Disposable>> Queue = new Queue<TaskCompletionSource<Disposable>>();

        private bool Busy;

        public Task<Disposable> LockAsync()
        {
            lock (Queue)
            {
                if (Busy)
                {
                    var completor = new TaskCompletionSource<Disposable>();
                    Queue.Enqueue(completor);
                    return completor.Task;
                }
                else
                {
                    Busy = true;
                    return Task.FromResult(new Disposable(this));
                }
            }
        }
    
        internal void Unlock()
        {
            lock (Queue)
            {
                if (Queue.Count > 0)
                {
                    Queue.Dequeue().TrySetResult(new Disposable(this));
                }
                else
                {
                    Busy = false;
                }
            }
        }

        public class Disposable : IDisposable
        {
            private AsyncLock Locker;

            internal Disposable(AsyncLock locker)
            {
                Locker = locker;
            }

            public void Dispose()
            {
                Locker?.Unlock();
                Locker = null;
            }
        }
    }
}
