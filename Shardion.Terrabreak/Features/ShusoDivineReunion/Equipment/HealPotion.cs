using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record HealPotion : IHeal
{
    public string Name => "Healing Potion";
    public string Description => "A true classic! Use to recover a moderate amount of HP.";
    public Tier Tier => Tier.Two;

    public int Heal(IPlayer wielder)
    {
        return 100;
    }
}
