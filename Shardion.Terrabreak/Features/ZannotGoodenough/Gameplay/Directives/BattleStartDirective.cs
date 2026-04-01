using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record BattleStartDirective(DirectiveSource Source, BattleStartInvocation Invocation) : IBattleDirective
{
    public string? LogLine => null;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic player1Relic in Invocation.Player1.GetRelicEnumerator())
        {
            Log.Debug("BattleStartDirective running for Player #1 relic {relic}.", player1Relic.Relic.Name);
            if (player1Relic.Relic.HookBattleStart(Invocation, player1Relic,
                    player1Relic.Relic.CastState(player1Relic.State)) is IEnumerable<IBattleDirective>
                player1RelicDirectives)
            {
                directives.AddRange(player1RelicDirectives);
            }
        }
        foreach (BattleRelic player2Relic in Invocation.Player2.GetRelicEnumerator())
        {
            Log.Debug("BattleStartDirective running for Player #2 relic {relic}.", player2Relic.Relic.Name);
            if (player2Relic.Relic.HookBattleStart(Invocation, player2Relic,
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
