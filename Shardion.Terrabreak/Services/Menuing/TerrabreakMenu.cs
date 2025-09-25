using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace Shardion.Terrabreak.Services.Menuing;

public abstract class TerrabreakMenu
{
    public Guid? MenuGuid { get; set; }
    public DateTimeOffset? CreationTime { get; set; } = DateTimeOffset.UtcNow;
    public IReadOnlySet<ulong>? AllowedUsers { get; set; }

    public abstract Task<MenuMessage> BuildMessage();

    public virtual async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        MenuGuid = guid;
        MenuMessage message = await BuildMessage();
        InteractionMessageProperties finalMessage = new InteractionMessageProperties()
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions);

        await RespondAsync(context, InteractionCallback.Message(finalMessage));
    }

    public virtual Task OnButton(ButtonInteractionContext context)
    {
        return Task.CompletedTask;
    }

    public Task<InteractionCallbackResponse?> RespondAsync(ApplicationCommandContext context,
        InteractionCallbackProperties callback, bool withResponse = false, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        return context.Interaction.SendResponseAsync(callback, withResponse, properties, cancellationToken);
    }

    public Task<RestMessage> GetResponseAsync(ApplicationCommandContext context,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.GetResponseAsync(properties, cancellationToken);
    }

    public Task<RestMessage> ModifyResponseAsync(ApplicationCommandContext context, Action<MessageOptions> action,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.ModifyResponseAsync(action, properties, cancellationToken);
    }

    public Task DeleteResponseAsync(ApplicationCommandContext context, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        return context.Interaction.DeleteResponseAsync(properties, cancellationToken);
    }

    public Task<RestMessage> FollowupAsync(ApplicationCommandContext context, InteractionMessageProperties message,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.SendFollowupMessageAsync(message, properties, cancellationToken);
    }

    public Task<RestMessage> GetFollowupAsync(ApplicationCommandContext context, ulong messageId,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.GetFollowupMessageAsync(messageId, properties, cancellationToken);
    }

    public Task<RestMessage> ModifyFollowupAsync(ApplicationCommandContext context, ulong messageId,
        Action<MessageOptions> action, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        return context.Interaction.ModifyFollowupMessageAsync(messageId, action, properties, cancellationToken);
    }

    public Task DeleteFollowupAsync(ApplicationCommandContext context, ulong messageId,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.DeleteFollowupMessageAsync(messageId, properties, cancellationToken);
    }

    public Task<InteractionCallbackResponse?> RespondAsync(IComponentInteractionContext context,
        InteractionCallbackProperties callback, bool withResponse = false, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        return context.Interaction.SendResponseAsync(callback, withResponse, properties, cancellationToken);
    }

    public Task<RestMessage> GetResponseAsync(IComponentInteractionContext context,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.GetResponseAsync(properties, cancellationToken);
    }

    public Task<RestMessage> ModifyResponseAsync(IComponentInteractionContext context, Action<MessageOptions> action,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.ModifyResponseAsync(action, properties, cancellationToken);
    }

    public Task DeleteResponseAsync(IComponentInteractionContext context, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        return context.Interaction.DeleteResponseAsync(properties, cancellationToken);
    }

    public Task<RestMessage> FollowupAsync(IComponentInteractionContext context, InteractionMessageProperties message,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.SendFollowupMessageAsync(message, properties, cancellationToken);
    }

    public Task<RestMessage> GetFollowupAsync(IComponentInteractionContext context, ulong messageId,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.GetFollowupMessageAsync(messageId, properties, cancellationToken);
    }

    public Task<RestMessage> ModifyFollowupAsync(IComponentInteractionContext context, ulong messageId,
        Action<MessageOptions> action, RestRequestProperties? properties = null,
        CancellationToken cancellationToken = default)
    {
        return context.Interaction.ModifyFollowupMessageAsync(messageId, action, properties, cancellationToken);
    }

    public Task DeleteFollowupAsync(IComponentInteractionContext context, ulong messageId,
        RestRequestProperties? properties = null, CancellationToken cancellationToken = default)
    {
        return context.Interaction.DeleteFollowupMessageAsync(messageId, properties, cancellationToken);
    }
}
