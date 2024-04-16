using Microsoft.Extensions.Hosting;

namespace Shardion.Terrabreak
{
    public interface ITerrabreakService
    {
        public Task StartAsync();
    }
}
