using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shardion.Terrabreak.Features.Bags;

public class BagEntryEntityTypeConfiguration : IEntityTypeConfiguration<BagEntry>
{
    public void Configure(EntityTypeBuilder<BagEntry> builder)
    {
        builder.HasOne(e => e.Bag).WithMany(e => e.Entries);
        builder.Navigation(e => e.Bag).AutoInclude().IsRequired();
    }
}
