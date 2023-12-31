﻿namespace Shared.Settings
{
    public class RabbitMqSettingsConst
    {
        public const string OrderSaga = "order-saga-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string StockReservedRequestPaymentQueueName = "stock-reserved-request-payment-queue";
        public const string OrderRequestCompletedQueueName = "order-request-completed-queue";
        public const string OrderRequestFailedEventQueueName = "order-request-failed-queue";
        public const string StockRollbackMessageQueueName = "stock-rollback-mqssage-queue";
    }
}
