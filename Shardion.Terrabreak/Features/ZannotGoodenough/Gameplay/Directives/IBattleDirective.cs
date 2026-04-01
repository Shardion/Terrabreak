using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public interface IBattleDirective
{
    public DirectiveSource Source { get; }
    public string? LogLine { get; }

    public IEnumerable<IBattleDirective>? FireHooks();
    public IEnumerable<IBattleDirective>? FireInterceptors();
    public IEnumerable<IBattleDirective>? Execute();
}
