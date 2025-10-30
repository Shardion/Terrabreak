using System.Collections.Generic;
using System.Threading.Tasks;
using NetCord.Rest;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;

public class InventoryMenu(IPlayer player) : TerrabreakMenu
{
    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Inventory")
        ];

        string emojiWeapon = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Weapon][(int)player.Weapon.Tier];
        components.Add(new TextDisplayProperties($"- {emojiWeapon} **{player.Weapon.Name}**\n> {player.Weapon.Description}"));

        string emojiShield = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Shield][(int)player.Shield.Tier];
        components.Add(new TextDisplayProperties($"- {emojiShield} **{player.Shield.Name}**\n> {player.Shield.Description}"));

        if (player.Heal is IHeal heal)
        {
            string emojiHeal = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Heal][(int)heal.Tier];
            components.Add(new TextDisplayProperties($"- {emojiHeal} **{heal.Name}**\n> {heal.Description}"));
        }

        if (player.Cure is ICure cure)
        {
            string emojiCure = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Cure][(int)cure.Tier];
            components.Add(new TextDisplayProperties($"- {emojiCure} **{cure.Name}**\n> {cure.Description}"));
        }

        if (player.Ribbons > 0)
        {
            components.Add(new TextDisplayProperties($"- <:ribbon:1429603809755398154> {player.Ribbons} **Ribbon{(player.Ribbons == 1 ? "" : "s")}**\n> As they say, knowledge is power! Use to destroy your enemies with facts and logic."));
        }

        if (player.Credits > 0)
        {
            components.Add(new TextDisplayProperties($"- <:credit:1426414005957689445> {player.Credits} **Credit{(player.Credits == 1 ? "" : "s")}**\n> A currency that can be used to buy items at the shop. Cannot be used to produce energy."));
        }

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(components)
        ]));
    }
}
