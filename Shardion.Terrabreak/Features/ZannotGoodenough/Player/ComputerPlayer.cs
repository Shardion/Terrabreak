namespace Shardion.Terrabreak.Features.ZannotGoodenough.Player;

public class ComputerPlayer : IPlayer
{
    public string Name { get; init; } = "Kasane Teto";

    public string GetMention()
    {
        return Name;
    }
}
