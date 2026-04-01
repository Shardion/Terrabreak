namespace Shardion.Terrabreak.Features.ZannotGoodenough;

public interface INamedEntity
{
    public string Name { get; }
    public string Description { get; }
    public string InternalName => Name.Replace(" ", "").Replace("-", "").Replace(".", "").Replace("!", "").Replace("'", "").Replace("\"", "");
}
