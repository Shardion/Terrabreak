using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shardion.Terrabreak.Features.Bags;

public class BagEntityTypeConfiguration : IEntityTypeConfiguration<Bag>
{
    public void Configure(EntityTypeBuilder<Bag> builder)
    {
        builder.HasMany(e => e.Entries).WithOne(e => e.Bag);
        builder.Navigation(e => e.Entries).AutoInclude();
    }
}
