using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Dtos;
using Order.Api.Models;
using Shared.Events;
using Shared.Interfaces;
using Shared.Settings;

namespace Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly DataContext _dataContext;

        public OrdersController(ISendEndpointProvider sendEndpointProvider, DataContext dataContext)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreateDto)
        {
            var newOrder = new Models.Order()
            {
                BuyerId = orderCreateDto.BuyerId,
                Status = Models.Status.Suspend,
                Address = new Models.Address()
                {
                    Line = orderCreateDto.Address.Line,
                    Province = orderCreateDto.Address.Province,
                    District = orderCreateDto.Address.District,
                },
                CreatedOn = DateTime.Now
            };

            orderCreateDto.OrderItems.ForEach(orderItem =>
            {
                newOrder.OrderItems.Add(new Models.OrderItem
                {
                    Price = orderItem.Price,
                    ProductId = orderItem.ProductId,
                    Count = orderItem.Count,
                });
            });

            await _dataContext.AddAsync(newOrder);
            await _dataContext.SaveChangesAsync();

            var orderCreatedRequestEvent = new OrderCreatedRequestEvent()
            {
                BuyerId = orderCreateDto.BuyerId,
                OrderId = newOrder.Id,
                Payment = new Shared.Messages.PaymentMessage
                {
                    CardName = orderCreateDto.Payment.CardName,
                    CardNumber = orderCreateDto.Payment.CardNumber,
                    CVV = orderCreateDto.Payment.CVV,
                    Expiration = orderCreateDto.Payment.Expiration,
                    TotalPrice = orderCreateDto.OrderItems.Sum(oi => oi.Count * oi.Price)
                }
            };
            orderCreateDto.OrderItems.ForEach(orderItem =>
            {
                orderCreatedRequestEvent.OrderItems.Add(new Shared.Messages.OrderItemMessage { Count = orderItem.Count, ProductId = orderItem.ProductId });
            });

            var sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettingsConst.OrderSaga}"));

            await sendEndPoint.Send<IOrderCreatedRequestEvent>(orderCreatedRequestEvent);

            return Ok();
        }
    }
}
