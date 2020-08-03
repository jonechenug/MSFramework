using Microsoft.EntityFrameworkCore;
using MSFramework.Common;
using MSFramework.Ef.Infrastructure;

namespace MSFramework.Ef.Extensions
{
	public static class ModelBuilderExtensions
	{
		public static ModelBuilder UseObjectId(this ModelBuilder modelBuilder)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var properties = entityType.GetProperties();
				foreach (var property in properties)
				{
					if (property.ClrType == typeof(ObjectId))
					{
						property.SetValueConverter(new ObjectIdToStringConverter());
					}
				}
			}

			return modelBuilder;
		}
	}
}