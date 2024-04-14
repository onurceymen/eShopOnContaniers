using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using Microsoft.Extensions.Logging;
using PaymentService.Api.IntegrationEvents.Events;

namespace PaymentService.Api.IntegrationEvents.EventHandlers
{
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IConfiguration _configuration;
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

        public OrderStartedIntegrationEventHandler(IConfiguration configuration, IEventBus eventBus, ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            _configuration = configuration;
            _eventBus = eventBus;
            this._logger = logger;
        }

        public Task Handle(OrderStartedIntegrationEvent @event)
        {
            string keyword = "PaymentSuccess";
            bool paymentSuccessFlag = _configuration.GetValue<bool>(keyword);

            IntegrationEvent paymetEvent = paymentSuccessFlag
                ? new OrderPaymentSuccessIntegrationEvent(@event.OrderId)
                : new OrderPaymentFailedIntegrationEvent(@event.OrderId, "This fake Error Message");
            _logger.LogInformation($"OrderStartedIntegrationEventHandler in PaymentService is fired with PaymentSuccess: {paymentSuccessFlag}, orderId: {@event.OrderId}");
            _eventBus.Publish(paymetEvent);

            return Task.CompletedTask;
        }
    }
}
