using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Quartz;
using Quartz.Impl.Matchers;
using Shardion.Terrabreak.Features.Bags;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.Reminders;

public class RemindersMenu(ISchedulerFactory schedulerFactory, ulong targetUser) : TerrabreakMenu
{
    public int PageEntryCount { get; set; } = 10;
    public int PageNumber { get; set; } = 0;

    public override async Task<MenuMessage> BuildMessage()
    {
        // TODO: No way to really tell what this does when there's multiple schedulers, so we just get any single one,
        //       and hope nothing blows up...?
        IScheduler scheduler = await schedulerFactory.GetScheduler();
        IReadOnlyCollection<JobKey> jobKeys = await scheduler.GetJobKeys(
            GroupMatcher<JobKey>.GroupEquals($"remindersFor{targetUser.ToString(CultureInfo.InvariantCulture)}"));

        // Integer division rounds towards zero
        int fullPages = jobKeys.Count / PageEntryCount;
        // Add page for remainder items
        bool remainderPage = jobKeys.Count % PageEntryCount > 0;
        int totalPages = remainderPage ? fullPages + 1 : fullPages;

        if (PageNumber + 1 > totalPages) PageNumber = totalPages - 1;
        if (PageNumber < 0) PageNumber = 0;

        if (totalPages <= 0)
        {
            return new MenuMessage([
                new ComponentContainerProperties()
                    .WithComponents([
                        new TextDisplayProperties("### Reminders"),
                        new TextDisplayProperties("-# (you have no reminders)")
                    ])
            ]);
        }

        IEnumerable<JobKey> pageReminders = jobKeys.Skip(PageNumber * PageEntryCount).Take(10);
        List<IComponentContainerComponentProperties> components = [];
        foreach (JobKey jobKey in pageReminders)
        {
            if (await scheduler.GetJobDetail(jobKey) is not IJobDetail jobDetail)
            {
                components.Add(
                    new TextDisplayProperties(
                        "- An error occurred while retrieving this entry's details <:ech:1417758642186485770>"));
                continue;
            }

            IReadOnlyCollection<ITrigger> jobTriggers = await scheduler.GetTriggersOfJob(jobKey);
            if (jobTriggers.FirstOrDefault() is not ITrigger jobTrigger)
            {
                components.Add(
                    new TextDisplayProperties(
                        "- An error occurred while retrieving this entry's activation info <:ech:1417758642186485770>"));
                continue;
            }

            components.Add(new ComponentSectionProperties(
                new ButtonProperties($"menu:{MenuGuid}:delete:{jobKey.Name}",
                    EmojiProperties.Custom(1417536968434389083),
                    ButtonStyle.Danger),
                [
                    new TextDisplayProperties(
                        $"- <t:{jobTrigger.StartTimeUtc.ToUnixTimeSeconds()}:F>\n>>> {jobDetail.JobDataMap["Note"]}")
                ]
            ));
        }

        if (totalPages > 1)
        {
            components.Insert(0, new TextDisplayProperties($"### Reminders, page {PageNumber + 1} / {totalPages}"));
            components.AddRange(new ComponentSeparatorProperties(),
                new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:page-previous", EmojiProperties.Custom(1417540510003757056),
                            ButtonStyle.Secondary)
                        .WithDisabled(PageNumber + 1 <= 1),
                    new ButtonProperties($"menu:{MenuGuid}:page-next", EmojiProperties.Custom(1417540508494073876),
                            ButtonStyle.Secondary)
                        .WithDisabled(PageNumber + 1 >= totalPages)
                ]));
        }
        else
        {
            components.Insert(0, new TextDisplayProperties($"### Reminders"));
        }

        return new MenuMessage([
            new ComponentContainerProperties()
                .WithComponents(components)
        ]);
    }

    public override async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        Task deferral = RespondAsync(context, InteractionCallback.DeferredMessage());
        MenuGuid = guid;
        MenuMessage message = await BuildMessage();

        await deferral;
        await ModifyResponseAsync(context, responseMessage =>
            responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        List<Task> tasks = [];
        string[] customIdFragments = context.Interaction.Data.CustomId.Split(":");
        string secondLastCustomIdFragment = customIdFragments[^2];
        string lastCustomIdFragment = customIdFragments[^1];
        if (secondLastCustomIdFragment.StartsWith("delete"))
        {
            IScheduler scheduler = await schedulerFactory.GetScheduler();
            tasks.Add(scheduler.DeleteJob(new JobKey(lastCustomIdFragment,
                $"remindersFor{targetUser.ToString(CultureInfo.InvariantCulture)}")));
        }
        else if (lastCustomIdFragment == "page-next")
        {
            PageNumber++;
        }
        else if (lastCustomIdFragment == "page-previous")
        {
            PageNumber--;
        }

        MenuMessage message = await BuildMessage();
        tasks.Add(RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions))));

        await Task.WhenAll(tasks);
    }
}
