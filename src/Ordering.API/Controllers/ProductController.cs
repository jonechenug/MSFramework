using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AspNetCore.AccessControl;
using MSFramework.AspNetCore.Mvc;
using MSFramework.Audits;
using MSFramework.Domain;
using MSFramework.Ef.Repositories;
using MSFramework.Mapper;
using MSFramework.Shared;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.API.Controllers
{
	public class CreateViewObject
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		[StringLength(50)]
		public string Name { get; set; }
	}

	public class ProductDTO
	{
		public string Name { get; private set; }

		public int Price { get; private set; }
	}

	public class MyBody
	{
		internal ObjectId Id { get; set; }
		public ObjectId MyId { get; set; }
		public string Name { get; set; }
	}

	[Route("api/v1.0/[controller]")]
	[ApiController]
	public class ProductController : ApiControllerBase
	{
		private readonly IProductRepository _productRepository;
		private readonly IRepository<AuditOperation> _repository;
		private readonly IObjectMapper _mapper;

		public ProductController(IProductRepository productRepository, IRepository<AuditOperation> repository,
			IObjectMapper mapper)
		{
			_productRepository = productRepository;
			_repository = repository;
			_mapper = mapper;
		}

		[HttpGet("objectid")]
		public ObjectId Get()
		{
			return ObjectId.NewId();
		}

		[HttpPost("objectid/{id}")]
		public MyBody Post([FromRoute] ObjectId id, [FromBody] MyBody body)
		{
			body.Id = id;
			return body;
		}

		[HttpGet("getAudits")]
		public List<AuditOperation> GetAudits()
		{
			Logger.LogInformation($"{Session.UserId}");
			return ((EfRepository<AuditOperation>) _repository).CurrentSet.Include(x => x.Entities).ToList();
		}

		[HttpGet("getBaseValueType")]
		public int GetBaseValueType()
		{
			return 1;
		}

		[HttpGet("getFirst1")]
		//[AccessControl("查看第一个产品", "产品")]
		public Product GetFirst1()
		{
			return _productRepository.GetFirst();
		}

		[HttpGet("getFirst2")]
		public Response<Product> GetFirst2()
		{
			var a = _productRepository.GetFirst();
			return new Response<Product>(a);
		}

		[HttpGet("getPagedQuery")]
		//[AccessControl("查询产品", "产品")]
		public async Task<Response<PagedResult<ProductDTO>>> GetPagedQuery()
		{
			PagedResult<Product> a = await _productRepository.PagedQueryAsync(0, 10);
			var b = _mapper.Map<PagedResult<ProductDTO>>(a);
			return new Response<PagedResult<ProductDTO>>(b);
		}

		[HttpGet("getError")]
		public Response GetErrorAsync()
		{
			return new ErrorResponse("I am an error response");
		}

		[HttpGet("getMSFrameworkException")]
		public Response GetMSFrameworkException()
		{
			throw new MSFrameworkException(2, "i'm framework exception");
		}

		[HttpGet("getException")]
		public Response GetExceptionAsync()
		{
			throw new Exception("i'm framework exception");
		}

		[HttpGet]
		public Product GetAsync(ObjectId productId)
		{
			return _productRepository.Get(productId);
		}

		[HttpDelete]
		//[AccessControl("删除产品", "产品")]
		public Product DeleteAsync(ObjectId productId)
		{
			return _productRepository.Delete(productId);
		}

		[HttpPost]
		public Product CreateAsync(CreateViewObject vo)
		{
			return _productRepository.Insert(new Product(vo.Name, new Random().Next(100, 10000)));
		}
	}
}