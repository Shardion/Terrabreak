using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Services.Menuing;

public class MenuButtonComponentsModule(MenuManager menuManager, IdentityManager identityManager)
    : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("menu")]
    public async Task RemoveBag(string serializedGuid, string componentName)
    {
        if (!Guid.TryParse(serializedGuid, out Guid guid))
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("(Internal error: failed to parse menu GUID)")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        TerrabreakMenu menu = menuManager.Menus[guid];
        if (menu.AllowedUsers is IReadOnlySet<ulong> allowedUsers)
        {
            if (!allowedUsers.Contains(Context.User.Id))
            {
                await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent(identityManager.GetAccessDeniedResponse())
                    .WithFlags(MessageFlags.Ephemeral)
                ));
            }
        }
        menu.LastInteractionTime = DateTimeOffset.UtcNow;
        await menuManager.Menus[guid].OnButton(Context);
    }

    public async Task ReplaceMenuAsync(TerrabreakMenu menu,
        bool withResponse = false, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        menu.LastInteractionTime = DateTimeOffset.UtcNow;
        await menu.OnReplace(Context, menuManager.ActivateMenu(menu));
    }
}
