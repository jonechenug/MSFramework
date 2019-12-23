using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.EventBus;

namespace MSFramework.Ef
{
	public abstract class DbContextBase : DbContext, IUnitOfWork
	{
		private readonly ILogger _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IEntityConfigurationTypeFinder _typeFinder;
		private readonly IEventBus _eventBus;

		public IMSFrameworkSession Session { get; internal set; }

		/// <summary>
		/// 初始化一个<see cref="DbContextBase"/>类型的新实例
		/// </summary>
		protected DbContextBase(DbContextOptions options, IEventBus eventBus,
			IEntityConfigurationTypeFinder typeFinder,
			ILoggerFactory loggerFactory)
			: base(options)
		{
			_typeFinder = typeFinder;
			_eventBus = eventBus;
			_loggerFactory = loggerFactory;
			_logger = loggerFactory?.CreateLogger(GetType());
		}

		/// <summary>
		/// 创建上下文数据模型时，对各个实体类的数据库映射细节进行配置
		/// </summary>
		/// <param name="modelBuilder">上下文数据模型构建器</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//通过实体配置信息将实体注册到当前上下文
			Type contextType = GetType();
			IEntityRegister[] registers = _typeFinder.GetEntityRegisters(contextType);
			foreach (IEntityRegister register in registers)
			{
				register.RegisterTo(modelBuilder);
				_logger?.LogDebug($"将实体类“{register.EntityType}”注册到上下文“{contextType}”中");
			}

			_logger?.LogInformation($"上下文“{contextType}”注册了{registers.Length}个实体类");
		}

		/// <summary>
		/// 模型配置
		/// </summary>
		/// <param name="optionsBuilder"></param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var options = EntityFrameworkOptions.EntityFrameworkOptionDict.Values.SingleOrDefault(m =>
				m.DbContextType == GetType());
			if (options != null && options.LazyLoadingProxiesEnabled)
			{
				optionsBuilder.UseLazyLoadingProxies();
			}

			if (_loggerFactory != null)
			{
				optionsBuilder.UseLoggerFactory(_loggerFactory);
			}

			if ("Development" == Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
			{
				optionsBuilder.EnableSensitiveDataLogging();
			}
		}

		/// <summary>
		///     将在此上下文中所做的所有更改保存到数据库中，同时自动开启事务或使用现有同连接事务
		/// </summary>
		/// <remarks>
		///     此方法将自动调用 <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> 
		///     若要在保存到基础数据库之前发现对实体实例的任何更改，请执行以下操作。这可以通过以下类型禁用
		///     <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		/// </remarks>
		/// <returns>
		///     写入数据库的状态项的数目。
		/// </returns>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateException">
		///     保存到数据库时遇到错误。
		/// </exception>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException">
		///     保存到数据库时会遇到并发冲突。
		///     当在保存期间影响到意外数量的行时，就会发生并发冲突。
		///     这通常是因为数据库中的数据在加载到内存后已经被修改。
		/// </exception>
		public override int SaveChanges()
		{
			try
			{
				var changed = ChangeTracker.Entries().Any();
				if (!changed)
				{
					return 0;
				}

				ApplyConcepts();

				var effectCount = base.SaveChanges();
				Database.CurrentTransaction?.Commit();
				return effectCount;
			}
			catch
			{
				Database.CurrentTransaction?.Rollback();
				throw;
			}
		}

		/// <summary>
		///     异步地将此上下文中的所有更改保存到数据库中，同时自动开启事务或使用现有同连接事务
		/// </summary>
		/// <remarks>
		///     <para>
		///         此方法将自动调用 <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> 
		///         若要在保存到基础数据库之前发现对实体实例的任何更改，请执行以下操作。这可以通过以下类型禁用
		///         <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
		///     </para>
		///     <para>
		///         不支持同一上下文实例上的多个活动操作。请使用“等待”确保在此上下文上调用其他方法之前任何异步操作都已完成。
		///     </para>
		/// </remarks>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     表示异步保存操作的任务。任务结果包含写入数据库的状态条目数。
		/// </returns>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateException">
		///     保存到数据库时遇到错误。
		/// </exception>
		/// <exception cref="T:Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException">
		///     保存到数据库时会遇到并发冲突。
		///     当在保存期间影响到意外数量的行时，就会发生并发冲突。
		///     这通常是因为数据库中的数据在加载到内存后已经被修改。
		/// </exception>
		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			try
			{
				var changed = ChangeTracker.Entries().Any();
				if (!changed)
				{
					return 0;
				}

				ApplyConcepts();

				// After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
				// performed through the DbContext will be committed

				var effectedCount = await base.SaveChangesAsync(cancellationToken);

				Database.CurrentTransaction?.Commit();

				return effectedCount;
			}
			catch
			{
				Database.CurrentTransaction?.Rollback();
				throw;
			}
		}

		public async Task<bool> CommitAsync()
		{
			// Dispatch Domain Events collection. 
			// Choices:
			// A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
			// side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
			// B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
			// You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
			var domainEntities = ChangeTracker
				.Entries<IEventProvider>()
				.Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any()).ToList();

			var domainEvents = domainEntities
				.SelectMany(x => x.Entity.DomainEvents)
				.ToList();

			domainEntities
				.ForEach(entity => entity.Entity.ClearDomainEvents());

			foreach (var @event in domainEvents)
			{
				await _eventBus.PublishAsync(@event);
			}

			// After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
			// performed through the DbContext will be committed
			await SaveChangesAsync();
			return true;
		}

		protected virtual void ApplyConcepts()
		{
			foreach (var entry in ChangeTracker.Entries())
			{
				ApplyConcepts(entry, Session?.UserId, Session?.UserName);
			}
		}

		protected virtual void ApplyConcepts(EntityEntry entry, string userId, string userName)
		{
			switch (entry.State)
			{
				case EntityState.Added:
					ApplyConceptsForAddedEntity(entry, userId, userName);
					break;
				case EntityState.Modified:
					ApplyConceptsForModifiedEntity(entry, userId, userName);
					break;
				case EntityState.Deleted:
					ApplyConceptsForDeletedEntity(entry, userId, userName);
					break;
			}
		}

		protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is ICreationAudited creationAudited)
			{
				creationAudited.SetCreationAudited(userId, userName);
			}
		}

		protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is IModificationAudited creationAudited)
			{
				creationAudited.SetModificationAudited(userId, userName);
			}
		}

		protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry, string userId, string userName)
		{
			if (entry.Entity is IDeletionAudited deletionAudited)
			{
				entry.Reload();
				entry.State = EntityState.Modified;

				deletionAudited.Delete(userId, userName);
			}
		}
	}
}