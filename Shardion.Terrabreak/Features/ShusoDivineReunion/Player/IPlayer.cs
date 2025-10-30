using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

public interface IPlayer
{
    public string Name { get; }
    public int HealthMax => Shield.IncreaseMaxHealth(100, this);

    public int Credits { get; set; }
    public IWeapon Weapon { get; }
    public IShield Shield { get; }
    public IHeal? Heal { get; }
    public ICure? Cure { get; }
    public int Ribbons { get; set; }
}
