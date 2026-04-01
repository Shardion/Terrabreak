using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class MissingChildIncident : IRelic<MissingChildIncidentState>
{
    public string Name => "Missing Child Incident";
    public string Description => "To the shadow realm with you, sucker!";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription => "Every turn, a random enemy monster is prohibited from using its Likes ability.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 1.0),
    ];

    public IEnumerable<IBattleDirective>? HookTurnStart(TurnStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (player.GetRelicEnumerator().Contains(thisRelic))
            {
                // Unlike the normal pattern, this relic activates if the player is NOT the one holding it
                continue;
            }

            MissingChildIncidentState state = (MissingChildIncidentState)thisState;
            state.MissingMonster = player.GetMonsterEnumerator()
                .Where(monster => !BattleRules.CheckKnockout(monster))
                .Shuffle()
                .FirstOrDefault();
        }

        return null;
    }

    public IEnumerable<IBattleDirective>? InterceptLikesAbility(LikesAbilityInvocation<MonsterState> invocation, BattleRelic thisRelic, RelicState thisState)
    {
        MissingChildIncidentState state = (MissingChildIncidentState)thisState;
        if (invocation.User == state.MissingMonster)
        {
            return [ new LogLineDirective(
                new(this, thisRelic, invocation.User),
                $"{invocation.FriendlyPlayer.Player.Name}'s {invocation.User.Monster.Name} was suspended for suspicious activity!"),
            ];
        }
        return null;
    }
}

public record MissingChildIncidentState : RelicState
{
    public BattleMonster? MissingMonster { get; set; }
}
