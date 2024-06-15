using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class TimeoutManager : ITerrabreakService, IDisposable
    {
        public event Func<Timeout, Task>? TimeoutExpired;

        private readonly IDbContextFactory<TerrabreakDatabaseContext> DatabaseContextFactory;

        private readonly Thread _timeoutExpiryThread;
        private readonly Thread _timeoutLoadingThread;

        private Task? _expiryThreadSleepTask;
        private Task? _loadingThreadSleepTask;

        private CancellationTokenSource _tokenSource;

        private bool _disposed;

        public TimeoutManager(IDbContextFactory<TerrabreakDatabaseContext> databaseContextFactory)
        {
            DatabaseContextFactory = databaseContextFactory;

            _tokenSource = new();
            _timeoutExpiryThread = new(CheckForExpiredTimeouts);
            _timeoutLoadingThread = new(LoadNewlyNearTimeouts);
        }

        private void CheckForExpiredTimeouts()
        {
            while (true)
            {
                Timeout? nearestTimeout = null;

                using (TerrabreakDatabaseContext context = DatabaseContextFactory.CreateDbContext())
                {
                    foreach (Timeout timeout in context.Set<Timeout>().Where(t => !t.ExpiryProcessed))
                    {
                        if (timeout.ExpirationDate < DateTimeOffset.UtcNow)
                        {
                            Task.Run(() =>
                            {
                                TimeoutExpired?.Invoke(timeout);
                            });
                            timeout.ExpiryProcessed = true;
                        }
                        else
                        {
                            if (nearestTimeout is null || timeout.ExpirationDate < nearestTimeout.ExpirationDate)
                            {
                                nearestTimeout = timeout;
                            }
                        }
                    }
                    context.SaveChanges();
                }

                if (nearestTimeout is not null)
                {
                    TimeSpan sleepTime = nearestTimeout.ExpirationDate - DateTimeOffset.UtcNow;
                    int sleepMillis = Convert.ToInt32(sleepTime.TotalMilliseconds);
                    if (sleepMillis > 0)
                    {
                        _expiryThreadSleepTask = Task.Delay(sleepMillis);
                    }
                    else
                    {
                        // #itjustworks
                        _expiryThreadSleepTask = Task.Delay(100);
                    }
                }
                else
                {
                    _expiryThreadSleepTask = Task.Delay(TimeSpan.FromMinutes(2));
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
                using (TerrabreakDatabaseContext context = DatabaseContextFactory.CreateDbContext())
                {
                    context.RemoveRange(context.Set<Timeout>().Where(t => t.ExpiryProcessed));
                    context.SaveChanges();
                }

                _loadingThreadSleepTask = Task.Delay(TimeSpan.FromMinutes(15));
                _loadingThreadSleepTask.Wait();
            }
        }

        public Timeout? GetTimeout(Guid timeoutGuid)
        {
            using TerrabreakDatabaseContext context = DatabaseContextFactory.CreateDbContext();
            return context.Set<Timeout>().Find(timeoutGuid);
        }

        public bool BeginTimeout(Guid timeoutGuid)
        {
            Timeout? timeout = GetTimeout(timeoutGuid);
            if (timeout is null)
            {
                return false;
            }
            return BeginTimeout(timeout);
        }

        public bool BeginTimeout(Timeout timeout)
        {
            using (TerrabreakDatabaseContext context = DatabaseContextFactory.CreateDbContext())
            {
                context.Add(timeout);
                context.SaveChanges();
            }
            _tokenSource.Cancel();
            return true;
        }

        public bool CancelTimeout(Guid timeoutGuid)
        {
            Timeout? timeout = GetTimeout(timeoutGuid);
            if (timeout is null)
            {
                return false;
            }
            return CancelTimeout(timeout);
        }

        public bool CancelTimeout(Timeout timeout)
        {
            using (TerrabreakDatabaseContext context = DatabaseContextFactory.CreateDbContext())
            {
                context.Remove(timeout);
                context.SaveChanges();
            }
            _tokenSource.Cancel();
            return true;
        }

        public bool UpdateTimeout(Timeout timeout)
        {
            using (TerrabreakDatabaseContext context = DatabaseContextFactory.CreateDbContext())
            {
                context.Update(timeout);
                context.SaveChanges();
            }
            _tokenSource.Cancel();
            return true;
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
                _tokenSource.Dispose();
            }

            _disposed = true;
        }
    }
}
