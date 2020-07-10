using AutoMapper;
using MSFramework.Common;
using Ordering.Domain.AggregateRoot;

namespace Ordering.API.Controllers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Product, ProductDTO>();
		}
	}
}