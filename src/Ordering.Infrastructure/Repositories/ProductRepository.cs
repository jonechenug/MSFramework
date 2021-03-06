using System.Linq;
using System.Threading.Tasks;
using MSFramework.Ef;
using MSFramework.Ef.Repositories;
using MSFramework.Extensions;
using MSFramework.Shared;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Infrastructure.Repositories
{
	public class ProductRepository : EfRepository<Product>, IProductRepository
	{
		public ProductRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}

		public Product GetFirst()
		{
			return CurrentSet.FirstOrDefault();
		}

		public async Task<PagedResult<Product>> PagedQueryAsync(int page, int limit)
		{
			return await CurrentSet.PagedQueryAsync(page, limit);
		}
	}
}