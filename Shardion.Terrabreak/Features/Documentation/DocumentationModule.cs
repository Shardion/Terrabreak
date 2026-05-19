using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NetCord;
using NetCord.JsonConverters;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Features.Documentation;

public class DocumentationModule(DocumentationManager documentationManager, IdentityOptions identity)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("changelog", "What's new?",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public Task Changelog(
        [SlashCommandParameter(Description = "The version associated with the changelog to view.")]
        string? version = null
    )
    {
        string? targetVersion = version ?? identity.CurrentVersion;
        if (targetVersion is null)
        {
            return RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Please specify a version!",
                Flags = MessageFlags.Ephemeral
            }
            ));
        }

        if (documentationManager.Changelogs.GetValueOrDefault(targetVersion) is not JsonDocument changelog)
        {
            return RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"No changelog exists for version `{version}`!",
                Flags = MessageFlags.Ephemeral
            }
            ));
        }

        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithComponents([
                new DumbComponent(changelog.RootElement)
            ])
            .WithFlags(MessageFlags.IsComponentsV2)
        ));
    }

    [SlashCommand("user-guide", "Learn how to use the app!",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public Task UserGuide()
    {
        if (documentationManager.UserGuide is not JsonComponent component)
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
            {
                Content = $"{identity.BotName} administration has not added a user guide!",
                Flags = MessageFlags.Ephemeral
            }
            ));

        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithComponents([

            ])
            .WithFlags(MessageFlags.IsComponentsV2)
        ));
    }
}
