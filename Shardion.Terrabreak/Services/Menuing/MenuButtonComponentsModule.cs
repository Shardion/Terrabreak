using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Services.Menuing;

public class MenuButtonComponentsModule(MenuManager menuManager, IdentityOptions identityOptions)
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
            if (!allowedUsers.Contains(Context.User.Id))
            {
                if (identityOptions.AccessDeniedResponses is string[] deniedResponses)
                    await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                        .WithContent(deniedResponses[Random.Shared.Next(deniedResponses.Length)])
                        .WithFlags(MessageFlags.Ephemeral)
                    ));
                else
                    await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                        .WithContent("Access denied.")
                        .WithFlags(MessageFlags.Ephemeral)
                    ));
                return;
            }

        await menuManager.Menus[guid].OnButton(Context);
    }
}
