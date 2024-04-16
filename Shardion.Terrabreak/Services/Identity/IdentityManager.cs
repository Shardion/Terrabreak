using Discord;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Identity
{
    public class IdentityManager : ITerrabreakService
    {
        private readonly OptionsManager _options;

        public IdentityManager(OptionsManager options)
        {
            _options = options;
        }

        public EmbedBuilder GetEmbedTemplate()
        {
            if (_options.Get<IdentityOptions>() is IdentityOptions identity)
            {
                return new EmbedBuilder()
                {
                    Color = identity.BotColor,
                };
            }
            else
            {
                return new EmbedBuilder();
            }
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
