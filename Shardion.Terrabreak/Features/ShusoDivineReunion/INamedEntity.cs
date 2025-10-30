namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public interface INamedEntity
{
    public string Name { get; }
    public string Description { get; }
    public string InternalName => Name.Replace(" ", "").Replace("-", "");
}
