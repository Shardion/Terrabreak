using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;

public class RetRat : IMonster<MonsterState>
{
    public string Name => "Ret Rat";
    public string Description => "Ret Rats like this one thrive in the presence of garbage, making waste management particularly important in the city.";
    public MonsterClassification Classification => MonsterClassification.Rodent;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Piercing;

    public int BaseHealth => 12;
    public int BaseAttack => 2;
    public int BaseDefense => 0;

    public ILikesAbility<MonsterState> LikesAbility { get; } = new LikesAbilityProliferate();
}

public class LikesAbilityProliferate : ILikesAbility<MonsterState>
{
    public string Name => "Proliferate";
    public string Description => "If space is available, creates a friendly Ret Rat.";
    public int BaseLikesCost => 10;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        BattleMonster newRetRat = new(new RetRat());
        if (arguments.FriendlyPlayer.Monster1 is null || BattleRules.CheckKnockout(arguments.FriendlyPlayer.Monster1))
        {
            arguments.FriendlyPlayer.Monster1 = newRetRat;
            return null;
        }
        if (arguments.FriendlyPlayer.Monster2 is null || BattleRules.CheckKnockout(arguments.FriendlyPlayer.Monster2))
        {
            arguments.FriendlyPlayer.Monster2 = newRetRat;
            return null;
        }
        if (arguments.FriendlyPlayer.Monster3 is null || BattleRules.CheckKnockout(arguments.FriendlyPlayer.Monster3))
        {
            arguments.FriendlyPlayer.Monster3 = newRetRat;
        }
        return null;
    }
}
