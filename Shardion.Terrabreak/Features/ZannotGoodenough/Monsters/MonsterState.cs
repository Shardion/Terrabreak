namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

public class MonsterState
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public int AttackStaticModifier { get; set; }
    public int AttackPercentageModifier { get; set; }

    public int DefenseStaticModifier { get; set; }
    public int DefensePercentageModifier { get; set; }

    public int LikesCostStaticModifier { get; set; }
    public int LikesCostPercentageModifier { get; set; }

    public double DodgeChance { get; set; }
}
