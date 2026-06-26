using Microsoft.AspNetCore.Mvc;
using RabbitMq.Domain.Enums;
using RabbitMQ.Application.DTOs;
using RabbitMQ.Application.Interfaces.Services.Orders;

namespace RabbitMq.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
		private readonly IOrderService _orderService;
		public OrdersController(IOrderService orderService)
        {
			_orderService = orderService;
        }

		[HttpGet]
		public async Task<IActionResult> GetAll(CancellationToken ct)
		{
			var orders = await _orderService.GetAllAsync(ct);

			return Ok(orders);
		}

		[HttpPost]
		public async Task<IActionResult> SubmitOrder(
		CreateOrderRequest request,
		CancellationToken ct)
		{
			var orderId = await _orderService.SubmitOrderAsync(request, ct);

			return Accepted(new
			{
				OrderId = orderId,
				Status = OrderStatus.Submitted
			});
		}
	}
}
