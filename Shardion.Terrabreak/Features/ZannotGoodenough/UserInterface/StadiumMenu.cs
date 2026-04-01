using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Stadium;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class StadiumMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player) : TerrabreakMenu
{
    public int Rounds { get; set; } = 1;

    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Stadium"),
        ];
        string loadoutLine = Loadout.ProduceLoadoutLine(emoji, player.EquippedLoadout);
        components.Add(new TextDisplayProperties($"Your loadout:\n- {loadoutLine}"));
        components.Add(new ActionRowProperties().WithComponents(
        [
            new ButtonProperties($"menu:{MenuGuid}:start", "Fight!", ButtonStyle.Primary),
            new ButtonProperties($"menu:{MenuGuid}:round", $"Rounds: {Rounds.ToString(CultureInfo.InvariantCulture)}", ButtonStyle.Secondary),
        ]));
        return Task.FromResult(new MenuMessage([new ComponentContainerProperties(components)]));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        if (player.DiscordUserId != context.User.Id)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent(identity.GetAccessDeniedResponse())
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Last() == "round")
        {
            Rounds = Rounds switch
            {
                1 => 3,
                3 => 5,
                5 => 7,
                7 => 10,
                _ => 1,
            };
        }
        else if (splitCustomId.Last() == "start")
        {
            await ReplaceMenuAsync(context, menu, new StadiumIntermissionMenu(dbContextFactory, identity, emoji, menu, player, new(Rounds, 0)));
            return;
        }

        MenuMessage message = await BuildMessage();
        await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions)));
    }
}
