using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record BattleStartInvocation(
    Battle Battlefield,
    BattleLoadout Player1,
    BattleLoadout Player2
)
{
    public IEnumerable<BattleLoadout> GetPlayerEnumerator()
    {
        yield return Player1;
        yield return Player2;
    }
}
