using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record HealSeltzer : IHeal
{
    public string Name => "Seltzer";
    public string Description => "A drink which bubbles intensely. Use to recover 150 HP.";
    public Tier Tier => Tier.Three;

    public int Heal(IPlayer wielder)
    {
        return 150;
    }
}
