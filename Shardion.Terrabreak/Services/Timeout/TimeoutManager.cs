using System.Collections.Concurrent;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class TimeoutManager
    {
        public event Func<Timeout, Task>? TimeoutExpired;

        private ConcurrentQueue<Timeout> MemoryTimeouts;
        private readonly TimeoutCollectionManager DatabaseTimeouts;

        private readonly CancellationTokenSource _tokenSource;
        private readonly Task? _waitForNextIntervalTask;

        public TimeoutManager(TimeoutCollectionManager databaseTimeouts)
        {
            DatabaseTimeouts = databaseTimeouts;

            _tokenSource = new();
        }

        private void CheckForExpiredTimeouts(CancellationToken token = default)
        {
            
        }

        public void BeginTimeout(Timeout timeout)
        {

        }

        private void RecalculateNextCheckTime()
        {
            
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
