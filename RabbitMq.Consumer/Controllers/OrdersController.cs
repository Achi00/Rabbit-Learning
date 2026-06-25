using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
using RabbitMQ.Application.DTOs;
using RabbitMqDemo.Persistance.Context;

namespace RabbitMq.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly MessageDbContext _db;

        public OrdersController(IPublishEndpoint publishEndpoint, MessageDbContext db)
        {
            _publishEndpoint = publishEndpoint;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                CustomerEmail = request.CustomerEmail,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _db.Orders.AddAsync(order);

            // goes through the outbox not sent to broker directly
            // MassTransit writes it to OutboxMessages table inside
            await _publishEndpoint.Publish(new OrderSubmitted(
                order.Id,
                order.CustomerEmail,
                order.Amount
            ));


            await _db.SaveChangesAsync();

            return Accepted(new { orderId = order.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order is null) return NotFound();
            return Ok(order);
        }
    }
}
