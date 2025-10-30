using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

public class HelperPlayer : IPlayer
{
    public required string Name { get; init; }

    public int Credits { get; set; }
    public IWeapon Weapon { get; init;  } = new WeaponWooden();
    public IShield Shield { get; init;  } = new ShieldCardboard();
    public IHeal? Heal { get; init; }
    public ICure? Cure { get; init; }
    public int Ribbons { get; set; }
}
