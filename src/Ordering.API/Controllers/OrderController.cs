using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MSFramework.Application;
using MSFramework.AspNetCore;
using MSFramework.AspNetCore.AccessControl;
using MSFramework.Shared;
using Ordering.Application.Commands;
using Ordering.Application.Queries;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;


namespace Ordering.API.Controllers
{
	[Route("api/v1.0/[controller]")]
	[ApiController]
	public class OrderController : ApiControllerBase
	{
		private readonly IOrderingQuery _orderingQuery;
		private readonly IOrderingRepository _orderRepository;
		private readonly IRequestProcessor _commandExecutor;

		public OrderController(IOrderingRepository orderRepository,
			IOrderingQuery orderingQuery, IRequestProcessor commandExecutor)
		{
			_orderingQuery = orderingQuery;
			_commandExecutor = commandExecutor;
			_orderRepository = orderRepository;
		}

		//[AccessControl("TestCreate")]
		[HttpPost("testCreate")]
		public async Task<IActionResult> TestCreate()
		{
			var order = new Order(
				"testUSer",
				new Address("Street", "City", "State", "Country", "ZipCode"),
				"Description",
				new List<OrderItem>
				{
					new OrderItem(Guid.NewGuid(),
						"testProduct", 10, 0, "")
				});
			await _orderRepository.InsertAsync(order);
			// var order = await _orderRepository.GetAsync(Guid.Parse("35a00497-cbb0-4311-af5d-ab6b01281569"));
			// order.AddOrderItem(Guid.NewGuid(),
			// 	"testProduct", 10, 0, "");
			// await _orderRepository.UpdateAsync(order);
			return Ok(order);
		}

		#region Command

		[HttpPost("test-command1")]
		//[AccessControl("test-command1")]
		public async Task<string> TestCommand1Async([FromBody] TestCommand1 command)
		{
			var a = await _commandExecutor.ProcessAsync(command, default);
			return a;
		}

		[HttpPost("test-command2")]
		//[AccessControl("test-command2")]
		public async Task TestCommand2Async([FromBody] TestCommand2 command)
		{
			await _commandExecutor.ProcessAsync(command, default);
		}

		/// <summary>
		/// FOR TEST Method
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		//[AccessControl("创建订单")]
		public async Task<ObjectId> CreateOrderAsync()
		{
			var random = new Random();
			var items = new List<CreateOrderCommand.OrderItemDTO>();
			var count = random.Next(2, 5);
			for (var i = 0; i < count; ++i)
			{
				var product = Guid.NewGuid();
				items.Add(new CreateOrderCommand.OrderItemDTO
				{
					ProductName = "product" + product.ToString("N"),
					ProductId = product,
					Units = random.Next(1, 10),
					Discount = 0,
					UnitPrice = random.Next(2, 1000)
				});
			}


			return await _commandExecutor.ProcessAsync(new CreateOrderCommand(items, "HELLO", "上海", "张扬路500号", "上海",
				"中国", "200000", "what?"));
		}

		[HttpDelete("{orderId}")]
		//[AccessControl("删除订单")]
		public IActionResult DeleteOrderAsync(Guid orderId)
		{
			// return await _mediator.Send(new DeleteOrderCommand(orderId));
			return Ok();
		}

		[HttpPut("{orderId}/address")]
		//[AccessControl("修改订单地址")]
		public IActionResult ChangeOrderAddressAsync([FromRoute] ObjectId orderId,
			[FromBody] ChangeOrderAddressCommand command)
		{
			command.OrderId = orderId;
			return Ok();
		}

		#endregion

		#region QUERY

		[HttpGet("{orderId}")]
		//[AccessControl("查看订单")]
		public async Task<IActionResult> GetOrderAsync([FromRoute] ObjectId orderId)
		{
			var order = await _orderingQuery.GetAsync(orderId);
			return Ok(order);
		}

		[HttpGet()]
		// [AccessControl("查看所有订单")]
		public async Task<IActionResult> GetOrdersAsync()
		{
			var order = await _orderingQuery.GetAllListAsync();
			return Ok(order);
		}

		#endregion
	}
}