using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class TimeoutManager : ITerrabreakService, IDisposable
    {
        public event Func<Timeout, Task>? TimeoutExpired;

        private readonly ConcurrentDictionary<Guid, Timeout> MemoryTimeouts;
        private readonly IDbContextFactory<TerrabreakDatabaseContext> DatabaseTimeoutsFactory;

        private readonly Thread _timeoutExpiryThread;
        private readonly Thread _timeoutLoadingThread;

        private Task? _expiryThreadSleepTask;
        private Task? _loadingThreadSleepTask;

        private CancellationTokenSource _tokenSource;
        private readonly Mutex _timeoutThreadMutex;

        private bool _disposed;

        public TimeoutManager(IDbContextFactory<TerrabreakDatabaseContext> databaseTimeoutsFactory)
        {
            MemoryTimeouts = [];
            DatabaseTimeoutsFactory = databaseTimeoutsFactory;

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

                using (TerrabreakDatabaseContext context = DatabaseTimeoutsFactory.CreateDbContext())
                {
                    foreach (Timeout timeout in MemoryTimeouts.Values)
                    {
                        if (!timeout.ExpiryProcessed)
                        {
                            if (timeout.ExpirationDate < DateTimeOffset.UtcNow)
                            {
                                Task.Run(() =>
                                {
                                    TimeoutExpired?.Invoke(timeout);
                                });
                                timeout.ExpiryProcessed = true;
                                context.Update(timeout);
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
                    context.SaveChanges();
                }

                _timeoutThreadMutex.ReleaseMutex();

                if (nearestTimeout is not null)
                {
                    TimeSpan sleepTime = nearestTimeout.ExpirationDate - DateTimeOffset.UtcNow;
                    if (sleepTime.TotalMicroseconds > 0)
                    {
                        _expiryThreadSleepTask = Task.Delay(sleepTime);
                    }
                    else
                    {
                        // #itjustworks
                        _expiryThreadSleepTask = Task.Delay(50);
                    }
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

                bool timerAdded = false;

                using (TerrabreakDatabaseContext context = DatabaseTimeoutsFactory.CreateDbContext())
                {
                    foreach (KeyValuePair<Guid, Timeout> pair in MemoryTimeouts.Where(timeout => timeout.Value.ExpiryProcessed))
                    {
                        _ = MemoryTimeouts.TryRemove(pair);
                    }
                    context.RemoveRange(context.Set<Timeout>().Where(t => t.ExpiryProcessed));

                    foreach (Timeout timeout in context.Set<Timeout>().AsEnumerable().Where(timeout => timeout.IsNear() && !timeout.ExpiryProcessed))
                    {
                        if (MemoryTimeouts.TryAdd(timeout.Id, timeout))
                        {
                            timerAdded = true;
                        }
                    }

                    context.SaveChanges();
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
            bool wakeUpProcessingThread = false;

            _timeoutThreadMutex.WaitOne();

            if (timeout.IsNear())
            {
                if (!MemoryTimeouts.TryAdd(timeout.Id, timeout))
                {
                    Log.Error("TimeoutManager tried to add new timeout to memory when it already exists!!!");
                }

                // Wakes up timeout processing thread which will adjust its sleep time to fit this timeout
                wakeUpProcessingThread = true;
            }

            using (TerrabreakDatabaseContext context = DatabaseTimeoutsFactory.CreateDbContext())
            {
                if (context.Find<Timeout>(timeout.Id) is not null)
                {
                    Log.Error("TimeoutManager tried to add new timeout to database when it already exists!!!");
                }
                else
                {
                    context.Add(timeout);
                    context.SaveChanges();
                }
            }

            _timeoutThreadMutex.ReleaseMutex();

            if (wakeUpProcessingThread)
            {
                _tokenSource.Cancel();
            }
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
