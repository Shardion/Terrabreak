using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record GrantLikesDirective(DirectiveSource Source, GrantLikesInvocation Invocation) : IBattleDirective
{
    public string? LogLine => null;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic friendlyRelic in Invocation.Player.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.InterceptGrantLikes(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        return directives;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        Invocation.Player.Likes += Invocation.Likes;
        return null;
    }
}
