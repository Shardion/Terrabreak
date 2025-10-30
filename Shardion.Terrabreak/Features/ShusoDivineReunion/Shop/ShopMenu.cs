using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Serilog;
using Shardion.Terrabreak.Features.Bags;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;

public class ShopMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identityManager) : TerrabreakMenu
{
    public ShopPurchase? PurchaseToMake { get; set; }
    public DiscordPlayer? TargetPlayer { get; set; }
    public SdrServer? TargetServer { get; set; }

    public static readonly string[] DefaultResponses =
    [
        "Hello, and welcome to my shop.",
        "BOX GOD's invaders are fierce, but you cannot lose!",
        "Use this equipment to defeat BOX GOD!",
    ];

    public static readonly string[] HalfwayResponses =
    [
        "Hello, and welcome back to my shop.",
        "Ah, it's you! Welcome back...!",
        "At this rate, we'll be able to challenge BOX GOD soon. You can do it!",
        "The factory? ...Don't worry about it.",
        "It isn't easy to get all of this. Handle it with care.",
        "Why did he have to pick Halloween, of all days...?",
    ];

    public static FrozenDictionary<Tier, IWeapon> WeaponsByTier { get; } = new Dictionary<Tier, IWeapon>([
        new(Tier.One, SdrRegistries.Weapons.Forward["WoodenBlade"]),
        new(Tier.Two, SdrRegistries.Weapons.Forward["UnionBlade"]),
        new(Tier.Three, SdrRegistries.Weapons.Forward["Neosaber"]),
        new(Tier.Four, SdrRegistries.Weapons.Forward["Boxcutter"]),
    ]).ToFrozenDictionary();

    public static FrozenDictionary<Tier, IShield> ShieldsByTier { get; } = new Dictionary<Tier, IShield>([
        new(Tier.One, SdrRegistries.Shields.Forward["CardboardShield"]),
        new(Tier.Two, SdrRegistries.Shields.Forward["SoldierShield"]),
        new(Tier.Three, SdrRegistries.Shields.Forward["HolyShield"]),
        new(Tier.Four, SdrRegistries.Shields.Forward["MantleMirror"]),
    ]).ToFrozenDictionary();

    public static FrozenDictionary<Tier, IHeal> HealsByTier { get; } = new Dictionary<Tier, IHeal>([
        new(Tier.One, SdrRegistries.Heals.Forward["Sandwich"]),
        new(Tier.Two, SdrRegistries.Heals.Forward["HealingPotion"]),
        new(Tier.Three, SdrRegistries.Heals.Forward["Seltzer"]),
        new(Tier.Four, SdrRegistries.Heals.Forward["RealPastry"]),
    ]).ToFrozenDictionary();

    public static FrozenDictionary<Tier, ICure> CuresByTier { get; } = new Dictionary<Tier, ICure>([
        new(Tier.One, SdrRegistries.Cures.Forward["Bread"]),
        new(Tier.Two, SdrRegistries.Cures.Forward["MegaBread"]),
        new(Tier.Three, SdrRegistries.Cures.Forward["GigaBread"]),
        new(Tier.Four, SdrRegistries.Cures.Forward["TeraBread"]),
    ]).ToFrozenDictionary();

    public static FrozenDictionary<Tier, int> CostsByTier { get; } = new Dictionary<Tier, int>([
        new(Tier.One, 25),
        new(Tier.Two, 100),
        new(Tier.Three, 1000),
        new(Tier.Four, 10000),
    ]).ToFrozenDictionary();

    public static FrozenDictionary<EquipmentType, string[]> EmojiListsByEquipmentType { get; } = new Dictionary<EquipmentType, string[]>([
        new(EquipmentType.Weapon, ["<:nothing:1424096863585308672>","<:weapon1:1426026980846862377>","<:weapon2:1426021784016126163>","<:weapon3:1426026982746886204>","<:weapon4:1426021787992592524>"]),
        new(EquipmentType.Shield, ["<:nothing:1424096863585308672>","<:shield1:1426026976816140369>","<:shield2:1426021770128916590>","<:shield3:1426021772691505305>","<:shield4:1426026979114881136>"]),
        new(EquipmentType.Heal, ["<:nothing:1424096863585308672>","<:heal1:1426021759609606225>","<:heal2:1426021761081806849>","<:heal3:1426021762604339231>","<:heal4:1426021764886167633>"]),
        new(EquipmentType.Cure, ["<:nothing:1424096863585308672>","<:cure1:1426021752659644446>","<:cure2:1426021754090029219>","<:cure3:1426021755717423224>","<:cure4:1426021758166630410>"]),
    ]).ToFrozenDictionary();

    public static FrozenDictionary<PurchaseType, string[]> EmojiListsByPurchaseType { get; } = new Dictionary<PurchaseType, string[]>([
        new(PurchaseType.Weapon, ["<:nothing:1424096863585308672>","<:weapon1:1426026980846862377>","<:weapon2:1426021784016126163>","<:weapon3:1426026982746886204>","<:weapon4:1426021787992592524>"]),
        new(PurchaseType.Shield, ["<:nothing:1424096863585308672>","<:shield1:1426026976816140369>","<:shield2:1426021770128916590>","<:shield3:1426021772691505305>","<:shield4:1426026979114881136>"]),
        new(PurchaseType.Heal, ["<:nothing:1424096863585308672>","<:heal1:1426021759609606225>","<:heal2:1426021761081806849>","<:heal3:1426021762604339231>","<:heal4:1426021764886167633>"]),
        new(PurchaseType.Cure, ["<:nothing:1424096863585308672>","<:cure1:1426021752659644446>","<:cure2:1426021754090029219>","<:cure3:1426021755717423224>","<:cure4:1426021758166630410>"]),
    ]).ToFrozenDictionary();

    public override async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        Task deferral = RespondAsync(context, InteractionCallback.DeferredMessage());
        MenuGuid = guid;

        if (context.Interaction.Guild is not Guild server)
        {
            throw new InvalidOperationException("Cannot build a shop message without a server to build from!");
        }

        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        Task<SdrServer> targetServerTask = dbContext.GetOrCreateServerAsync(server.Id);
        TargetPlayer = await dbContext.GetOrCreatePlayerFromUserAsync(context.Interaction.User);
        TargetServer = await targetServerTask;

        MenuMessage message = await BuildMessage();

        await deferral;
        await ModifyResponseAsync(context, responseMessage =>
            responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions));
    }

    public override Task<MenuMessage> BuildMessage()
    {
        if (TargetPlayer is null)
        {
            throw new InvalidOperationException("Cannot build a shop message without a player to build from!");
        }

        if (TargetServer is null)
        {
            throw new InvalidOperationException("Cannot build a shop message without a server to build from!");
        }

        if (PurchaseToMake is not null)
        {
            if (PurchaseToMake.Kind == PurchaseType.Ribbon || PurchaseToMake.Kind == PurchaseType.Passage)
            {
                return Task.FromResult(BuildResourcePurchaseMessage(TargetPlayer, TargetServer, PurchaseToMake));
            }
            return Task.FromResult(BuildEntityPurchaseMessage(TargetPlayer, PurchaseToMake));
        }

        return Task.FromResult(BuildListMessage(TargetPlayer, TargetServer));
    }

    private MenuMessage BuildEntityPurchaseMessage(DiscordPlayer player, ShopPurchase purchase)
    {
        if (purchase.Tier is null || purchase.Entity is null)
        {
            throw new InvalidOperationException("Cannot build an entity purchase message for a non-entity purchase!");
        }
        string emojiPurchase = EmojiListsByPurchaseType[purchase.Kind][(int)purchase.Tier];
        string emojiWeapon = EmojiListsByEquipmentType[EquipmentType.Weapon][(int)player.Weapon.Tier];
        string emojiShield = EmojiListsByEquipmentType[EquipmentType.Shield][(int)player.Shield.Tier];
        string emojiHeal = EmojiListsByEquipmentType[EquipmentType.Heal][(int)(player.Heal?.Tier ?? Tier.Zero)];
        string emojiCure = EmojiListsByEquipmentType[EquipmentType.Cure][(int)(player.Cure?.Tier ?? Tier.Zero)];
        return new MenuMessage([
            new ComponentContainerProperties([
                new TextDisplayProperties("### The Shop"),
                new TextDisplayProperties("Buy this item?"),
                new ComponentSeparatorProperties
                {
                    Spacing = ComponentSeparatorSpacingSize.Large,
                },
                new TextDisplayProperties($"### {emojiPurchase} {purchase.Entity.Name}"),
                new TextDisplayProperties($">>> {purchase.Entity.Description}"),
                new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:confirm", purchase.Cost.ToString(CultureInfo.InvariantCulture), EmojiProperties.Custom(1426414005957689445), ButtonStyle.Success)
                    {
                        Disabled = player.Credits < purchase.Cost
                    },
                    new ButtonProperties($"menu:{MenuGuid}:cancel", EmojiProperties.Custom(1426440266742501476), ButtonStyle.Danger),
                ]),
                new ComponentSeparatorProperties
                {
                    Spacing = ComponentSeparatorSpacingSize.Large,
                },
                new TextDisplayProperties($"-# {emojiWeapon}{emojiShield}{emojiHeal}{emojiCure}"),
                new TextDisplayProperties($"-# <:credit:1426414005957689445> {player.Credits}"),
            ])
        ]);
    }

    private MenuMessage BuildResourcePurchaseMessage(DiscordPlayer player, SdrServer server, ShopPurchase purchase)
    {
        if (purchase.Kind != PurchaseType.Ribbon && purchase.Kind != PurchaseType.Passage)
        {
            throw new InvalidOperationException("Cannot build an entity purchase message for a non-entity purchase!");
        }
        string emojiWeapon = EmojiListsByEquipmentType[EquipmentType.Weapon][(int)player.Weapon.Tier];
        string emojiShield = EmojiListsByEquipmentType[EquipmentType.Shield][(int)player.Shield.Tier];
        string emojiHeal = EmojiListsByEquipmentType[EquipmentType.Heal][(int)(player.Heal?.Tier ?? Tier.Zero)];
        string emojiCure = EmojiListsByEquipmentType[EquipmentType.Cure][(int)(player.Cure?.Tier ?? Tier.Zero)];

        string name;
        string emoji;
        string description;

        if (purchase.Kind == PurchaseType.Ribbon)
        {
            name = "Ribbon";
            emoji = "<:ribbon:1429603809755398154>";
            description = "As they say, knowledge is power!\nUse to destroy your enemies with facts and logic.";
        }
        else
        {
            name = "Passage";
            emoji = "<:passage:1431869229665226782>";
            description = "A random fact, a morsel of wisdom, or a piece of nonsense.\nShared by all players.";
        }

        bool allPassagesUnlocked = purchase.Kind == PurchaseType.Passage && server.PassagesUnlocked.Count >= SdrRegistries.Passages.Count;
        bool playerLacksFunds = player.Credits < purchase.Cost;
        bool buyingDisabled = playerLacksFunds || allPassagesUnlocked;

        return new MenuMessage([
            new ComponentContainerProperties([
                new TextDisplayProperties("### The Shop"),
                new TextDisplayProperties("Buy this item?"),
                new ComponentSeparatorProperties
                {
                    Spacing = ComponentSeparatorSpacingSize.Large,
                },
                new TextDisplayProperties($"### {emoji} {name}"),
                new TextDisplayProperties($">>> {description}"),
                new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:confirm", purchase.Cost.ToString(CultureInfo.InvariantCulture), EmojiProperties.Custom(1426414005957689445), ButtonStyle.Success)
                    {
                        Disabled = buyingDisabled,
                    },
                    new ButtonProperties($"menu:{MenuGuid}:cancel", EmojiProperties.Custom(1426440266742501476), ButtonStyle.Danger),
                ]),
                new ComponentSeparatorProperties
                {
                    Spacing = ComponentSeparatorSpacingSize.Large,
                },
                new TextDisplayProperties($"-# {emojiWeapon}{emojiShield}{emojiHeal}{emojiCure}"),
                new TextDisplayProperties($"-# <:credit:1426414005957689445> {player.Credits}"),
            ])
        ]);
    }

    private MenuMessage BuildListMessage(DiscordPlayer player, SdrServer server)
    {
        string response;
        if (player.StrongestEnemy is EnemyRecord { Enemy.TargetTotalPowerLevel: >= 4.0 })
        {
            response = HalfwayResponses[Random.Shared.Next(HalfwayResponses.Length)];
        }
        else
        {
            response = DefaultResponses[Random.Shared.Next(DefaultResponses.Length)];
        }

        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties($"### The Shop"),
            new TextDisplayProperties($"> *{response}*"),
            new ComponentSeparatorProperties(),
        ];
        components.AddRange(ProduceShopInventory(player, server));

        string emojiWeapon = EmojiListsByEquipmentType[EquipmentType.Weapon][(int)player.Weapon.Tier];
        string emojiShield = EmojiListsByEquipmentType[EquipmentType.Shield][(int)player.Shield.Tier];
        string emojiHeal = EmojiListsByEquipmentType[EquipmentType.Heal][(int)(player.Heal?.Tier ?? Tier.Zero)];
        string emojiCure = EmojiListsByEquipmentType[EquipmentType.Cure][(int)(player.Cure?.Tier ?? Tier.Zero)];
        components.AddRange([
            new ComponentSeparatorProperties(),
            new TextDisplayProperties($"-# {emojiWeapon}{emojiShield}{emojiHeal}{emojiCure}"),
            new TextDisplayProperties($"-# <:credit:1426414005957689445> {player.Credits}"),
        ]);

        return new MenuMessage([new ComponentContainerProperties(components)]);
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        List<Task> tasks = [];
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        if (TargetPlayer is null)
        {
            throw new InvalidOperationException("Cannot build a shop message without a player to build from!");
        }
        TargetPlayer = dbContext.GetPlayer(TargetPlayer.UserId);
        if (TargetPlayer is null)
        {
            throw new InvalidOperationException("(Still) cannot build a shop message without a player to build from!");
        }

        if (TargetServer is null)
        {
            throw new InvalidOperationException("Cannot build a shop message without a player to build from!");
        }
        TargetServer = dbContext.GetServer(TargetServer.ServerId);
        if (TargetServer is null)
        {
            throw new InvalidOperationException("(Still) cannot build a shop message without a player to build from!");
        }

        string[] customIdFragments = context.Interaction.Data.CustomId.Split(":");
        string lastCustomIdFragment = customIdFragments[^1];
        if (lastCustomIdFragment == "confirm")
        {
            if (PurchaseToMake is not ShopPurchase purchase)
            {
                throw new InvalidOperationException("Cannot confirm a purchase that does not exist!");
            }

            if (TargetPlayer.Credits < purchase.Cost)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent(identityManager.GetAccessDeniedResponse())
                    .WithFlags(MessageFlags.Ephemeral)
                ));
                return;
            }
            if (purchase.Entity is not null && purchase.Tier is not null)
            {
                if (purchase.Kind == PurchaseType.Weapon)
                {
                    TargetPlayer.WeaponId = purchase.Entity.InternalName;
                }
                else if (purchase.Kind == PurchaseType.Shield)
                {
                    TargetPlayer.ShieldId = purchase.Entity.InternalName;
                }
                else if (purchase.Kind == PurchaseType.Heal)
                {
                    TargetPlayer.HealId = purchase.Entity.InternalName;
                }
                else if (purchase.Kind == PurchaseType.Cure)
                {
                    TargetPlayer.CureId = purchase.Entity.InternalName;
                }
            }
            else if (purchase.Kind == PurchaseType.Ribbon)
            {
                TargetPlayer.Ribbons++;
            }
            else if (purchase.Kind == PurchaseType.Passage)
            {
                int? maybeFirstLockedPassage = null;
                for (int passageIndex = 0; passageIndex < SdrRegistries.Passages.Count; passageIndex++)
                {
                    if (!TargetServer.PassagesUnlocked.Contains(passageIndex))
                    {
                        maybeFirstLockedPassage = passageIndex;
                        break;
                    }
                }

                if (maybeFirstLockedPassage is not int firstLockedPassage)
                {
                    throw new InvalidOperationException("Cannot purchase a new passage when all passages are unlocked!");
                }

                TargetServer.PassagesUnlocked.Add(firstLockedPassage);
                dbContext.Update(TargetServer);
            }

            TargetPlayer.Credits -= purchase.Cost;
            dbContext.Update(TargetPlayer);
            tasks.Add(dbContext.SaveChangesAsync());
            PurchaseToMake = null;
        }
        else if (lastCustomIdFragment == "cancel")
        {
            PurchaseToMake = null;
        }
        else
        {
            string secondLastCustomIdFragment = customIdFragments[^2];

            string typeString;
            string? tierString = null;
            if (lastCustomIdFragment == "Ribbon" || lastCustomIdFragment == "Passage")
            {
                typeString = lastCustomIdFragment;
            }
            else
            {
                typeString = secondLastCustomIdFragment;
                tierString = lastCustomIdFragment;
            }


            PurchaseType purchaseType = Enum.Parse<PurchaseType>(typeString);
            INamedEntity? entity = null;
            Tier? maybeTier = null;
            int cost = 500;
            if (ConvertPurchaseType(purchaseType) is EquipmentType equipmentType)
            {
                Tier tier = Enum.Parse<Tier>(tierString ?? throw new InvalidOperationException("Can't produce equipment for a null tier!"));
                maybeTier = tier;
                entity = GetListForEquipmentType(equipmentType)[tier];
                cost = CostsByTier[tier];
            }

            PurchaseToMake = new ShopPurchase
            {
                Tier = maybeTier,
                Kind = purchaseType,
                Cost = cost,
                Entity = entity,
            };
        }

        MenuMessage message = await BuildMessage();
        tasks.Add(RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions))));
        await Task.WhenAll(tasks);
    }

    private List<ComponentSectionProperties> ProduceShopInventory(DiscordPlayer player, SdrServer server)
    {
        Tier healTier = player.Heal?.Tier ?? Tier.Zero;
        Tier cureTier = player.Cure?.Tier ?? Tier.Zero;
        bool passagesAvailable = server.PassagesUnlocked.Count < SdrRegistries.Passages.Count;

        List<ComponentSectionProperties?> maybeLines = [
            ProduceShopItem(EquipmentType.Weapon, player.Weapon.Tier, WeaponsByTier),
            ProduceShopItem(EquipmentType.Shield, player.Shield.Tier, ShieldsByTier),
            ProduceShopItem(EquipmentType.Heal, healTier, HealsByTier),
            ProduceShopItem(EquipmentType.Cure, cureTier, CuresByTier),
            new(
                new ButtonProperties($"menu:{MenuGuid}:Ribbon", "500", EmojiProperties.Custom(1426414005957689445), ButtonStyle.Success),
                [new("- <:ribbon:1429603809755398154> Ribbon")]
                ),
        ];
        if (passagesAvailable)
        {
            maybeLines.Add(new(
                new ButtonProperties($"menu:{MenuGuid}:Passage", "500", EmojiProperties.Custom(1426414005957689445), ButtonStyle.Success),
                [new("- <:passage:1431869229665226782> Passage")]
            ));
        }
        List<ComponentSectionProperties> lines = [];
        foreach (ComponentSectionProperties? maybeLine in maybeLines)
        {
            if (maybeLine is ComponentSectionProperties line)
            {
                lines.Add(line);
            }
        }

        return lines;
    }

    private ComponentSectionProperties? ProduceShopItem<TEntity>(EquipmentType equipmentType, Tier previousTier, IReadOnlyDictionary<Tier, TEntity> equipmentTable) where TEntity : INamedEntity
    {
        Tier? maybeTier = IncrementTier(previousTier);
        if (maybeTier is not Tier tier)
        {
            return null;
        }

        string emoji = EmojiListsByEquipmentType[equipmentType][(int)tier];
        TEntity entity = equipmentTable[tier];

        return new(
            new ButtonProperties($"menu:{MenuGuid}:{Enum.GetName(equipmentType)}:{Enum.GetName(tier)}", CostsByTier[tier].ToString(CultureInfo.InvariantCulture), EmojiProperties.Custom(1426414005957689445), ButtonStyle.Success),
            [new TextDisplayProperties($"- {emoji} {entity.Name}")]
        );
    }

    private static Tier? IncrementTier(Tier tier) =>
        tier switch
        {
            Tier.Zero => Tier.One,
            Tier.One => Tier.Two,
            Tier.Two => Tier.Three,
            Tier.Three => Tier.Four,
            _ => null,
        };

    private static EquipmentType? ConvertPurchaseType(PurchaseType purchaseType) =>
        purchaseType switch
        {
            PurchaseType.Weapon => EquipmentType.Weapon,
            PurchaseType.Shield => EquipmentType.Shield,
            PurchaseType.Heal => EquipmentType.Heal,
            PurchaseType.Cure => EquipmentType.Cure,
            _ => null,
        };

    private static PurchaseType? ConvertEquipmentType(EquipmentType equipmentType) =>
        equipmentType switch
        {
            EquipmentType.Weapon => PurchaseType.Weapon,
            EquipmentType.Shield => PurchaseType.Shield,
            EquipmentType.Heal => PurchaseType.Heal,
            EquipmentType.Cure => PurchaseType.Cure,
            _ => null,
        };


    private FrozenDictionary<Tier, INamedEntity> GetListForEquipmentType(EquipmentType equipmentType) =>
        equipmentType switch
        {
            EquipmentType.Weapon => WeaponsByTier.Values.ToFrozenDictionary<IWeapon, Tier, INamedEntity>(entity => entity.Tier, entity => entity),
            EquipmentType.Shield => ShieldsByTier.Values.ToFrozenDictionary<IShield, Tier, INamedEntity>(entity => entity.Tier, entity => entity),
            EquipmentType.Heal => HealsByTier.Values.ToFrozenDictionary<IHeal, Tier, INamedEntity>(entity => entity.Tier, entity => entity),
            EquipmentType.Cure => CuresByTier.Values.ToFrozenDictionary<ICure, Tier, INamedEntity>(entity => entity.Tier, entity => entity),
            _ => throw new ArgumentOutOfRangeException(nameof(equipmentType), equipmentType, null)
        };
}
