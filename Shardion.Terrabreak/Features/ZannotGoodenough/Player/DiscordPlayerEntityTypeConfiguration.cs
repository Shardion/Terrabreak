using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Player;

public class DiscordPlayerEntityTypeConfiguration : IEntityTypeConfiguration<DiscordPlayer>
{
    public void Configure(EntityTypeBuilder<DiscordPlayer> builder)
    {
        builder.OwnsOne(player => player.Loadout1);
        builder.OwnsOne(player => player.Loadout2);
        builder.OwnsOne(player => player.Loadout3);
    }
}
