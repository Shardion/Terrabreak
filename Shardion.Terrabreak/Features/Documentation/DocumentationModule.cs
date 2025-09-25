using System.Collections.Generic;
using System.Threading.Tasks;
using NetCord;
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
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
                {
                    Content = "Please specify a version!",
                    Flags = MessageFlags.Ephemeral
                }
            ));

        if (documentationManager.Changelogs.GetValueOrDefault(targetVersion) is not JsonComponent changelog)
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
                {
                    Content = $"No changelog exists for version `{version}`!",
                    Flags = MessageFlags.Ephemeral
                }
            ));

        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithComponents([
                new DumbComponent(changelog)
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
                new DumbComponent(component)
            ])
            .WithFlags(MessageFlags.IsComponentsV2)
        ));
    }
}
