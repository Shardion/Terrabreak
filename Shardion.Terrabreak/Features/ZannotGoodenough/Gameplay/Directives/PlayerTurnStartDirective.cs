using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record PlayerTurnStartDirective(DirectiveSource Source, PlayerTurnStartInvocation Invocation) : IBattleDirective
{
    public string? LogLine => null;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleMonster player1Monster in Invocation.FriendlyPlayer.GetMonsterEnumerator())
        {
            if (player1Monster.Monster.HookPlayerTurnStart(Invocation, player1Monster,
                    player1Monster.Monster.CastState(player1Monster.State)) is IEnumerable<IBattleDirective>
                player1MonsterDirectives)
            {
                directives.AddRange(player1MonsterDirectives);
            }
        }
        foreach (BattleMonster player2Monster in Invocation.EnemyPlayer.GetMonsterEnumerator())
        {
            if (player2Monster.Monster.HookPlayerTurnStart(Invocation, player2Monster,
                    player2Monster.Monster.CastState(player2Monster.State)) is IEnumerable<IBattleDirective>
                player2MonsterDirectives)
            {
                directives.AddRange(player2MonsterDirectives);
            }
        }
        foreach (BattleRelic player1Relic in Invocation.FriendlyPlayer.GetRelicEnumerator())
        {
            if (player1Relic.Relic.HookPlayerTurnStart(Invocation, player1Relic,
                    player1Relic.Relic.CastState(player1Relic.State)) is IEnumerable<IBattleDirective>
                player1RelicDirectives)
            {
                directives.AddRange(player1RelicDirectives);
            }
        }
        foreach (BattleRelic player2Relic in Invocation.EnemyPlayer.GetRelicEnumerator())
        {
            if (player2Relic.Relic.HookPlayerTurnStart(Invocation, player2Relic,
                    player2Relic.Relic.CastState(player2Relic.State)) is IEnumerable<IBattleDirective>
                player2RelicDirectives)
            {
                directives.AddRange(player2RelicDirectives);
            }
        }

        return directives;
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        return null;
    }
}
