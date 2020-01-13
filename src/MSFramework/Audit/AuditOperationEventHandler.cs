using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace MSFramework.Audit
{
	public class AuditOperationEventHandler : IEventHandler<AuditOperationEvent>
	{
		private readonly IServiceProvider _serviceProvider;

		public AuditOperationEventHandler(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task Handle(AuditOperationEvent @event)
		{
			// 开新的 scope 避免和 httpcontext 的 dbcontext 混用
			using var scope = _serviceProvider.CreateScope();
			var auditStore = scope.ServiceProvider.GetRequiredService<IAuditStore>();
			await auditStore.SaveAsync(@event.AuditOperation);
			scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>().CommitAsync().GetAwaiter();
		}
	}
}