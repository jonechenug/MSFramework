using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Audit;
using MSFramework.Ef;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class AuditOperationConfiguration
		: EntityTypeConfigurationBase<AuditOperation>
	{
		public override Type DbContextType => typeof(OrderingContext);
	}
}