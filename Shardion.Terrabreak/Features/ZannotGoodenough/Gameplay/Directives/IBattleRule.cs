using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public interface IBattleRule<out TRuleResult, out TInvocation> where TRuleResult : IRuleResult
{
    public IEnumerable<TInvocation> FireHooks();
    public IEnumerable<TRuleResult> FireInterceptors();
    public TRuleResult Execute();

    public IBattleRule<TRuleResult, TInvocation> Modify(object maybeInvocation);
}
