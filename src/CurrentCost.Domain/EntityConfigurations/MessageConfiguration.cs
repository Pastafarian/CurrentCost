using CurrentCost.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrentCost.Domain.EntityConfigurations
{
	public class MessageConfiguration : IEntityTypeConfiguration<Message>
	{
		public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasIndex(x => x.CreatedTime).IsDescending();
			builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedTime);
			builder.Property(x => x.Src).HasMaxLength(100);
        }
	}
}
