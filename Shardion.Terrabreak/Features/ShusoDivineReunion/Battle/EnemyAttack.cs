using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public record EnemyAttack(string AttackName, IReadOnlyCollection<Debuff> Debuffs, int Damage, int Targets, int? UntargetedDamage = null, IReadOnlyCollection<Debuff>? UntargetedDebuffs = null);
