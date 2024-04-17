using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Features.ListSpot
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    public class ListSpotModule : InteractionModuleBase
    {
        private readonly IdentityManager _identity;

        public ListSpotModule(IdentityManager options)
        {
            _identity = options;
        }

        [SlashCommand("list-spot", "Posts an embed showing the song you're currently listening to on Spotify")]
        public async Task ListSpot()
        {
            SpotifyGame? spotify = null;
            foreach (IActivity activity in Context.User.Activities)
            {
                if (activity is SpotifyGame locatedSpotify)
                {
                    spotify = locatedSpotify;
                    break;
                }
            }

            if (spotify is null)
            {
                await RespondAsync("You don't seem to be listening to a song on Spotify. Have you connected your Spotify and Discord accounts?", ephemeral: true);
                return;
            }

            string shownUserName;
            if (Context.User is IGuildUser guildUser)
            {
                shownUserName = guildUser.Nickname ?? guildUser.DisplayName ?? guildUser.Username;
            }
            else
            {
                shownUserName = Context.User.GlobalName ?? Context.User.Username;
            }

            EmbedBuilder embedBuilder = _identity.GetEmbedTemplate()
                .WithAuthor($"{shownUserName} is listening to...")
                .WithThumbnailUrl(spotify.AlbumArtUrl)
                .WithTitle($"{string.Join(", ", spotify.Artists)} — {spotify.TrackTitle}")
                .WithDescription($"On *{spotify.AlbumTitle}*");

            await RespondAsync(embed: embedBuilder.Build());
        }
    }
}
