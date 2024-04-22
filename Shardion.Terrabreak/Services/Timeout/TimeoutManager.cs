using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class TimeoutManager : ITerrabreakService, IDisposable
    {
        public event Func<Timeout, Task>? TimeoutExpired;

        private readonly ConcurrentDictionary<Guid, Timeout> MemoryTimeouts;
        private readonly TimeoutCollectionManager DatabaseTimeouts;

        private readonly Thread _timeoutExpiryThread;
        private readonly Thread _timeoutLoadingThread;

        private Task? _expiryThreadSleepTask;
        private Task? _loadingThreadSleepTask;

        private CancellationTokenSource _tokenSource;
        private readonly Mutex _timeoutThreadMutex;

        private bool _disposed;

        public TimeoutManager(TimeoutCollectionManager databaseTimeouts)
        {
            MemoryTimeouts = [];
            DatabaseTimeouts = databaseTimeouts;

            _timeoutThreadMutex = new();
            _tokenSource = new();
            _timeoutExpiryThread = new(CheckForExpiredTimeouts);
            _timeoutLoadingThread = new(LoadNewlyNearTimeouts);
        }

        private void CheckForExpiredTimeouts()
        {
            while (true)
            {
                _timeoutThreadMutex.WaitOne();

                Timeout? nearestTimeout = null;
                foreach (Timeout timeout in MemoryTimeouts.Values)
                {
                    if (!timeout.ExpiryProcessed)
                    {
                        if (timeout.ExpirationDate < DateTimeOffset.UtcNow)
                        {
                            timeout.ExpiryProcessed = true;
                            DatabaseTimeouts.Collection.Update(timeout);
                            Task.Run(() =>
                            {
                                TimeoutExpired?.Invoke(timeout);
                            });
                        }
                        else
                        {
                            if (nearestTimeout is null || timeout.ExpirationDate.CompareTo(nearestTimeout.ExpirationDate) < 0)
                            {
                                nearestTimeout = timeout;
                            }
                        }
                    }
                }

                _timeoutThreadMutex.ReleaseMutex();

                if (nearestTimeout is not null)
                {
                    TimeSpan sleepTime = nearestTimeout.ExpirationDate - DateTimeOffset.UtcNow;
                    _expiryThreadSleepTask = Task.Delay(sleepTime);
                }
                else
                {
                    _expiryThreadSleepTask = Task.Delay(System.Threading.Timeout.Infinite);
                }

                try
                {
                    _expiryThreadSleepTask.Wait(_tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation "wakes up" the thread on command
                    // Resetting the source doesn't really seem to work, so we just make a new one
                    // #itjustworks
                    _tokenSource = new();
                }
            }
        }

        private void LoadNewlyNearTimeouts()
        {
            while (true)
            {
                _timeoutThreadMutex.WaitOne();

                foreach (KeyValuePair<Guid, Timeout> pair in MemoryTimeouts.Where(timeout => timeout.Value.ExpiryProcessed))
                {
                    _ = MemoryTimeouts.TryRemove(pair);
                }
                //DatabaseTimeouts.Collection.DeleteMany(timeout => timeout.ExpiryProcessed);

                bool timerAdded = false;

                foreach (Timeout timeout in DatabaseTimeouts.Collection.FindAll().Where(timeout => timeout.IsNear() && !timeout.ExpiryProcessed))
                {
                    if (MemoryTimeouts.TryAdd(timeout.Id, timeout))
                    {
                        timerAdded = true;
                    }
                }

                _timeoutThreadMutex.ReleaseMutex();

                if (timerAdded)
                {
                    _tokenSource.Cancel();
                }

                _loadingThreadSleepTask = Task.Delay(TimeSpan.FromMinutes(15));
                _loadingThreadSleepTask.Wait();
            }
        }

        public void BeginTimeout(Timeout timeout)
        {
            if (timeout.IsNear())
            {
                if (!MemoryTimeouts.TryAdd(timeout.Id, timeout))
                {
                    Log.Error("TimeoutManager tried to add new timeout to memory when it already exists!!!");
                }

                // Wakes up timeout processing thread which will adjust its sleep time to fit this timeout
                _tokenSource.Cancel();
            }
            if (DatabaseTimeouts.Collection.FindById(timeout.Id) is not null)
            {
                Log.Error("TimeoutManager tried to add new timeout to database when it already exists!!!");
            }
            _ = DatabaseTimeouts.Collection.Insert(timeout);
        }

        public Task StartAsync()
        {
            _timeoutLoadingThread.Start();
            _timeoutExpiryThread.Start();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposingManagedObjects: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedObjects)
        {
            if (_disposed)
            {
                return;
            }

            if (disposingManagedObjects)
            {
                _timeoutThreadMutex.Dispose();
                _tokenSource.Dispose();
            }

            _disposed = true;
        }
    }
}
