using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Ef;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Infrastructure.EntityConfigurations
{
	public class OrderEntityTypeConfiguration : EntityTypeConfigurationBase<Order, OrderingContext>
	{
		public override void Configure(EntityTypeBuilder<Order> builder)
		{
			base.Configure(builder);

			builder.HasKey(o => o.Id);

			//Address value object persisted as owned entity type supported since EF Core 2.0
			builder.OwnsOne(o => o.Address);

			builder.Property<DateTimeOffset>("CreationTime").IsRequired();
			builder.Property<bool>("IsDeleted").IsRequired();
			builder.Property<string>("UserId").IsRequired();
			builder.Property<string>("Description").IsRequired(false);
			builder.Property<OrderStatus>("OrderStatus").IsRequired();

			var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));

			// DDD Patterns comment:
			//Set as field (New since EF 1.1) to access the OrderItem collection property through its field
			navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
		}
	}
}