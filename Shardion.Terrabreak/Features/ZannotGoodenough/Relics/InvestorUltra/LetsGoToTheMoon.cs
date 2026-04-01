using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;

public class LetsGoToTheMoon : IRelic<RelicState>
{
    public string Name => "Let's Go To The Moon!";
    public string InternalName => "LetsGoToTheMoon";
    public string Description => "";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorUltra"];
    public string EffectDescription => "If you gain more than 4 Likes at once, you gain another 2 Likes.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Likes, 1.0),
    ];

    public IEnumerable<IBattleDirective>? InterceptGrantLikes(GrantLikesInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        if (!invocation.Player.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }

        if (invocation.Likes > 4)
        {
            return
            [
                new GrantLikesDirective(
                    new(this, thisRelic, null),
                    invocation with { Likes = 2 }
                )
            ];
        }

        return null;
    }
}
