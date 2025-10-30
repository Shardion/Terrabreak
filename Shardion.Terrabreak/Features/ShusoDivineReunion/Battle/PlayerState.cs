using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public sealed class PlayerState
{
    public int Health { get; set; } = 100;
    public ISet<Debuff> Debuffs { get; } = new HashSet<Debuff>();
    public Intervention? QueuedIntervention { get; set; }
    public int CureUses { get; set; }
    public int HealUses { get; set; }
    public int RibbonUses { get; set; }
}
