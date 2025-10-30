using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

public class DiscordPlayerEntityTypeConfiguration : IEntityTypeConfiguration<DiscordPlayer>
{
    public void Configure(EntityTypeBuilder<DiscordPlayer> builder)
    {
    }
}
