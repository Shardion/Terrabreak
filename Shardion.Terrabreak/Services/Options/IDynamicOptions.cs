namespace Shardion.Terrabreak.Services.Options;

public interface IDynamicOptions
{
    public string SectionName { get; }
    public OptionsPermissions Permissions { get; }
}
