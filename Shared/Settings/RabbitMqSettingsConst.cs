namespace Shared.Settings
{
    public class RabbitMqSettingsConst
    {
        public const string OrderSaga = "order-saga-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string StockReservedRequestPaymentQueueName = "stock-reserved-request-payment-queue";
        public const string OrderRequestCompletedQueueName = "order-request-completed-queue";
    }
}
