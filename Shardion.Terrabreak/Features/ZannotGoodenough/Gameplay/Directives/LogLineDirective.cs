using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record LogLineDirective(DirectiveSource Source, string? LogLine) : IBattleDirective
{
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
        return null;
    }
}
