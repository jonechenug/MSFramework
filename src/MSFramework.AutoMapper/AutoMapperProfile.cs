using AutoMapper;
using MSFramework.Shared;

namespace MSFramework.AutoMapper
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
		}
	}
}