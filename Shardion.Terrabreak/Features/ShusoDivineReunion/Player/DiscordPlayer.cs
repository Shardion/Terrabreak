using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NetCord;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

[Index(nameof(UserId), IsUnique = true)]
public sealed class DiscordPlayer : IPlayer
{
    [Key]
    public Guid Id { get; init; }
    public ulong UserId { get; init; }
    public string Name { get; set; } = "Kasane Teto"; // TODO: This really, really isn't the best solution...

    public int Credits { get; set; }
    public int Ribbons { get; set; }
    public string WeaponId { get; set; } = "WoodenBlade";
    public string ShieldId { get; set; } = "CardboardShield";
    public string? HealId { get; set; }
    public string? CureId { get; set; }
    public EnemyRecord? StrongestEnemy { get; set; }

    public IWeapon Weapon => SdrRegistries.Weapons.Forward[WeaponId];
    public IShield Shield => SdrRegistries.Shields.Forward[ShieldId];
    public IHeal? Heal => HealId is not null ? SdrRegistries.Heals.Forward[HealId] : null;
    public ICure? Cure => CureId is not null ? SdrRegistries.Cures.Forward[CureId] : null;
}
