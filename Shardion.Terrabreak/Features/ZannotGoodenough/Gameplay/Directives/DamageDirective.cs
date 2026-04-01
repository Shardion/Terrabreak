using System;
using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record DamageDirective(DirectiveSource Source, DamageInvocation Invocation, string? LogLine = null) : IBattleDirective
{
    public string? LogLine { get; private set; } = LogLine;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        Log.Debug("Damage executed against {monster} dealing {damage} damage.", Invocation.Defender.Monster.Name, Invocation.FinalDamage);
        Invocation.Defender.State.CurrentHealth = Math.Max(0, Invocation.Defender.State.CurrentHealth - Invocation.FinalDamage);
        return null;
    }
}
