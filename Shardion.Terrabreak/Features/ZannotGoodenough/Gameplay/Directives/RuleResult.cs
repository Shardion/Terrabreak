using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public interface IRuleResult
{
    public IEnumerable<IBattleDirective> Directives { get; }
}
