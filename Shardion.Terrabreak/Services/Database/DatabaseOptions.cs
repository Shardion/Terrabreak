using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Database;

public sealed class DatabaseOptions : IStaticOptions
{
    public string SectionName => "Database";

    public required string ConnectionString { get; set; }
}
