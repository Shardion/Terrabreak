using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Serilog;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak;

// Intentionally abstract to prevent being loaded
public abstract class TerrabreakApplicationCommandModule(MenuManager menuManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    public async Task ActivateMenuAsync(TerrabreakMenu menu,
        bool withResponse = false, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        await menu.OnCreate(Context, menuManager.ActivateMenu(menu));
        /*InteractionMessageProperties finalMessage = new InteractionMessageProperties()
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions);

        await RespondAsync(InteractionCallback.Message(finalMessage), cancellationToken: cancellationToken, withResponse: withResponse, properties: properties);*/
    }
}
