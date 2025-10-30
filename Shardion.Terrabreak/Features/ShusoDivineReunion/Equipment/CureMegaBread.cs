using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public class CureMegaBread : ICure
{
    public string Name => "Mega Bread";
    public string Description => "An extra-large loaf of French bread. Use to cure all debuffs two times.";
    public Tier Tier => Tier.Two;

    public int GetMaxUses(IPlayer wielder) => 2;
}
