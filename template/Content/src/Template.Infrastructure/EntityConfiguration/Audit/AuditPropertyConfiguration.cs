using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Template.Infrastructure.EntityConfiguration.Audit
{
	public class AuditPropertyConfiguration
		: EntityTypeConfigurationBase<AuditProperty, AppDbContext>
	{
		public override void Configure(EntityTypeBuilder<AuditProperty> builder)
		{
			base.Configure(builder);

			builder.ToTable("audit_property");
			
			builder.Property(x => x.Id).HasColumnName("id");
			builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
			builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(255);
			builder.Property(x => x.NewValue).HasColumnName("new_value");
			builder.Property(x => x.OriginalValue).HasColumnName("original_value");
		}
	}
}