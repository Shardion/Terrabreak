using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class DuelMenu(IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer invoker, DiscordPlayer opponent) : TerrabreakMenu
{
    public override Task<MenuMessage> BuildMessage()
    {
        BattleLoadout invokerLoadout = BattleLoadout.FromLoadout(invoker.EquippedLoadout, invoker);
        BattleLoadout opponentLoadout = BattleLoadout.FromLoadout(opponent.EquippedLoadout, opponent);
        string invokerLoadoutLine = BattleLoadout.ProduceLoadoutLine(emoji, invokerLoadout);
        string opponentLoadoutLine = BattleLoadout.ProduceLoadoutLine(emoji, opponentLoadout);

        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Duel"),
            new TextDisplayProperties($"{opponent.GetMention()}!!"),
            new TextDisplayProperties($"{invoker.GetMention()} is challenging you to a battle!"),
            new TextDisplayProperties($"**{invoker.Name}**'s loadout:\n- {invokerLoadoutLine}"),
            new TextDisplayProperties($"**{opponent.Name}**'s loadout:\n- {opponentLoadoutLine}"),
            new ComponentSeparatorProperties(),
            new ActionRowProperties([
                new ButtonProperties($"menu:{MenuGuid}:accept", "Accept", ButtonStyle.Success),
                new ButtonProperties($"menu:{MenuGuid}:decline", "Decline", ButtonStyle.Danger),
            ]),
        ];

        return Task.FromResult(new MenuMessage([new ComponentContainerProperties(components)]));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        if (opponent.DiscordUserId != context.User.Id)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent(identity.GetAccessDeniedResponse())
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Last() == "decline")
        {
            BattleLoadout invokerLoadout = BattleLoadout.FromLoadout(invoker.EquippedLoadout, invoker);
            BattleLoadout opponentLoadout = BattleLoadout.FromLoadout(opponent.EquippedLoadout, opponent);
            string invokerLoadoutLine = BattleLoadout.ProduceLoadoutLine(emoji, invokerLoadout);
            string opponentLoadoutLine = BattleLoadout.ProduceLoadoutLine(emoji, opponentLoadout);

            List<IComponentContainerComponentProperties> components = [
                new TextDisplayProperties("### Duel"),
                new TextDisplayProperties($"{opponent.GetMention()} rejected the duel with {invoker.GetMention()}."),
                new TextDisplayProperties($"**{invoker.Name}**'s loadout:\n- {invokerLoadoutLine}"),
                new TextDisplayProperties($"**{opponent.Name}**'s loadout:\n- {opponentLoadoutLine}"),
                new ComponentSeparatorProperties(),
                new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:accept", "Accept", ButtonStyle.Success)
                    {
                        Disabled = true,
                    },
                    new ButtonProperties($"menu:{MenuGuid}:decline", "Decline", ButtonStyle.Danger)
                    {
                        Disabled = true,
                    },
                ]),
            ];

            MenuMessage message = new([new ComponentContainerProperties(components)]);
            await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions)));
            return;
        }

        if (splitCustomId.Last() == "accept")
        {
            Battle battle = new()
            {
                Player1 = BattleLoadout.FromLoadout(invoker.EquippedLoadout, invoker),
                Player2 = BattleLoadout.FromLoadout(opponent.EquippedLoadout, opponent),
            };
            await ReplaceMenuAsync(context, menu, new InvestmentMenu(battle, identity, emoji, menu, null));
        }
    }
}
