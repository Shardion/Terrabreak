using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;

public class BroccoliMan : IMonster<MonsterState>
{
    public string Name => "Broccoli Man";
    public string Description => "A sentient species of magical broccoli, these fantastic creatures are populous, yet elusive, thought to bring luck to those who find them.";
    public MonsterClassification Classification => MonsterClassification.Nature;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Haunting;

    public int BaseHealth => 16;
    public int BaseAttack => 2;
    public int BaseDefense => 2;

    public ILikesAbility<MonsterState> LikesAbility { get; } = new LikesAbilityWhereToFindIt();
}

public class LikesAbilityWhereToFindIt : ILikesAbility<MonsterState>
{
    public string Name => "Where To Find It";
    public string Description => "Generates 6 likes.";
    public int BaseLikesCost => 3;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        return
        [
            new GrantLikesDirective(
                new(this, null, arguments.User),
                new(arguments.Battlefield, arguments.FriendlyPlayer, 6)
            ),
        ];
    }
}
