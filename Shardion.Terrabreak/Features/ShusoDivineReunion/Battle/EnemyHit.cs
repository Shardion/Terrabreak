using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public record EnemyHit(string AttackName, IReadOnlyCollection<Debuff> Debuffs, int Damage);
