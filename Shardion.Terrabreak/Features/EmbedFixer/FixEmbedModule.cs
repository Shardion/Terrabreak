using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Services.Options;
using Shardion.Terrabreak.Utilities;

namespace Shardion.Terrabreak.Features.EmbedFixer;

public class FixEmbedModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Fix Embed")]
    public async Task FixEmbed(RestMessage message)
    {
        ReadOnlyCollection<string> foundLinks = TextParsingUtil.FindLinks(message.Content);

        if (foundLinks.Count <= 0)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("No links found in this message!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        StringBuilder links = new();
        foreach (string link in foundLinks)
            if (TextParsingUtil.RedirectToFixerService(link) is string fixedLink)
                links.AppendLine(fixedLink);

        if (links.Length <= 0)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Could not fix any embeds in this message!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent(links.ToString())
            .WithFlags(MessageFlags.Ephemeral)
        ));
    }
}
