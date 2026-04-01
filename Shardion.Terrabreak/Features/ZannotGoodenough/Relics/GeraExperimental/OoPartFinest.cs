using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.GeraExperimental;

public class OoPartFinest : IRelic<OoPartFinestState>
{
    public string Name => "Oo Part's Finest";
    public string InternalName => "OoPartFinest";
    public string Description =>
        "Help, I can't think of anything for this description, and the event launches in 15 minutes!! —S";
    public IRelicSeries Series => Registries.RelicSeries.Forward["GeraExperimental"];

    public string EffectDescription =>
        "After being attacked once, friendly Spirit-class monsters will dodge all attacks for the remainder of the turn.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 0.5),
        new(RelicCategory.SpiritMonster, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookTurnStart(TurnStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        OoPartFinestState state = (OoPartFinestState)thisState;
        state.AttackedMonsters.Clear();
        return null;
    }

    public DodgingResult? InterceptDodge(DodgingInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        if (!invocation.DefendingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }

        if (invocation.Defender.Monster.Classification != MonsterClassification.Spirit)
        {
            return null;
        }

        OoPartFinestState state = (OoPartFinestState)thisState;
        if (state.AttackedMonsters.Contains(invocation.Defender))
        {
            return new([], true);
        }

        state.AttackedMonsters.Add(invocation.Defender);
        return null;
    }
}

public record OoPartFinestState : RelicState
{
    public HashSet<BattleMonster> AttackedMonsters { get; } = [];
}
