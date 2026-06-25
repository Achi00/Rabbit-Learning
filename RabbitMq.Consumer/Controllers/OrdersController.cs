using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMq.Contracts.Events;
using RabbitMq.Domain.Entity;
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

        
    }
}
