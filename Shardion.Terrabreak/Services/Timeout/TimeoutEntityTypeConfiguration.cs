using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shardion.Terrabreak.Services.Timeout;

namespace Shardion.Terrabreak.Features.Reminders
{
    public class RemindersEntityTypeConfiguration : IEntityTypeConfiguration<Timeout>
    {
        public void Configure(EntityTypeBuilder<Timeout> builder)
        {

        }
    }
}
