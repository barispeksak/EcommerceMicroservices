using MassTransit;
using Common.Contracts.Commands;
using Common.Contracts.Events;
using Common.Contracts.Timeouts;
using OrderSagaOrchestrator.Data;
using System;

namespace OrderSagaOrchestrator;

public class OrderStateMachine : MassTransitStateMachine<SagaState>
{
    public State WaitingForStock { get; private set; } = null!;
    public State WaitingForOrder { get; private set; } = null!;
    public State WaitingForPayment { get; private set; } = null!; 
    public State Completed { get; private set; } = null!;

    public Event<FinalStockCheckRequested> StartOrder { get; private set; } = null!;
    public Event<StockReserved> StockOk { get; private set; } = null!;
    public Event<StockNotAvailable> StockFail { get; private set; } = null!;
    public Event<OrderCreated> OrderOk { get; private set; } = null!;
    public Event<OrderFailed> OrderFail { get; private set; } = null!;
    
    public Event<PaymentProcessed> PaymentOk { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFail { get; private set; } = null!;
    public Event<PaymentTimeoutReached> PaymentTimeout { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => StartOrder, e => 
        {
            e.CorrelateById(context => context.Message.CorrelationId);
            e.InsertOnInitial = true;
            e.SetSagaFactory(context => new SagaState
            {
                CorrelationId = context.Message.CorrelationId,
                CartId = context.Message.CartId,
                OrderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });
        });
        
        Event(() => StockOk, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => StockFail, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => OrderOk, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => OrderFail, x => x.CorrelateById(context => context.Message.CorrelationId));
        
        Event(() => PaymentOk, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentFail, x => x.CorrelateById(context => context.Message.CorrelationId));
        
        Event(() => PaymentTimeout, x => x.CorrelateById(context => context.Message.CorrelationId));

        Initially(
            When(StartOrder)
                .Then(ctx => 
                {
                    Console.WriteLine($" Starting order saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" CartId: {ctx.Saga.CartId}");
                    Console.WriteLine($" Checking stock availability...");
                })
                .TransitionTo(WaitingForStock)
        );

        During(WaitingForStock,
            When(StockOk)
                .Then(ctx => {
                    Console.WriteLine($" Stock reserved for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" Requesting order creation...");
                })
                .Publish(ctx => new CreateOrderRequested(ctx.Saga.CartId)
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .TransitionTo(WaitingForOrder)
                .Then(ctx => Console.WriteLine($" Transitioned to WaitingForOrder state")),

            When(StockFail)
                .Then(ctx => {
                    Console.WriteLine($" Stock not available for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" Finalizing saga due to stock unavailability");
                })
                .Finalize()
        );

        During(WaitingForOrder,
            When(OrderOk)
                .Then(ctx => {
                    Console.WriteLine($" Order created successfully for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" OrderId: {ctx.Message.OrderId}");
                    Console.WriteLine($" Processing payment...");
                    
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                })
                .Publish(ctx => new ProcessPaymentRequested
                {
                    CartId = ctx.Saga.CartId,
                    OrderId = ctx.Message.OrderId,
                    Amount = 99.99m, 
                    PaymentTypeId = 1, 
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .TransitionTo(WaitingForPayment)
                .Then(ctx => Console.WriteLine($" Transitioned to WaitingForPayment state for saga {ctx.Saga.CorrelationId}")),
                
            When(OrderFail)
                .Then(ctx => {
                    Console.WriteLine($" Order creation failed for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" Reason: {ctx.Message.Reason}");
                    Console.WriteLine($" Starting compensation - releasing reserved stock...");
                })
                .Publish(ctx => new RollbackStock(ctx.Saga.CartId)
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .Then(ctx => Console.WriteLine($" Stock rollback sent, finalizing saga due to order failure"))
                .Finalize()
        );

        During(WaitingForPayment,
            When(PaymentOk)
                .Then(ctx => {
                    Console.WriteLine($" Payment processed successfully for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" Order saga completed successfully!");
                    
                    // TODO: Add order completion logic here:
                    // - Update order status to "Completed"
                    // - Send notification to user
                    // - Update inventory
                    // - Release reserved stock
                    // - Send notification to user
                })
                .Finalize(),
                
            When(PaymentFail)
                .Then(ctx => {
                    Console.WriteLine($" Payment failed for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" Reason: {ctx.Message.Reason}");
                    Console.WriteLine($" Starting compensation - canceling order and releasing stock...");
                })
                .Publish(ctx => new CancelOrder(ctx.Saga.CartId)
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .Publish(ctx => new RollbackStock(ctx.Saga.CartId)
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .Then(ctx => Console.WriteLine($" Compensation commands sent, finalizing saga due to payment failure"))
                .Finalize(),
                
            When(PaymentTimeout)
                .Then(ctx => {
                    Console.WriteLine($" Payment timeout reached for saga {ctx.Saga.CorrelationId}");
                    Console.WriteLine($" Starting compensation - canceling order and releasing stock...");
                })
                .Publish(ctx => new CancelOrder(ctx.Saga.CartId)
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .Publish(ctx => new RollbackStock(ctx.Saga.CartId)
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    SourceService = "OrderSaga"
                })
                .Then(ctx => Console.WriteLine($" Compensation commands sent, finalizing saga due to payment timeout"))
                .Finalize()
        );

        DuringAny(
            When(StockOk)
                .Then(ctx => Console.WriteLine($" StockOk received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring")),
            When(StockFail)
                .Then(ctx => Console.WriteLine($" StockFail received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring")),
            When(OrderOk)
                .Then(ctx => Console.WriteLine($" OrderOk received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring")),
            When(OrderFail)
                .Then(ctx => Console.WriteLine($" OrderFail received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring")),
            When(PaymentOk)
                .Then(ctx => Console.WriteLine($" PaymentOk received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring")),
            When(PaymentFail)
                .Then(ctx => Console.WriteLine($" PaymentFail received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring")),
            When(PaymentTimeout)
                .Then(ctx => Console.WriteLine($" PaymentTimeout received for saga {ctx.Saga.CorrelationId} in state {ctx.Saga.CurrentState} - ignoring"))
        );

        SetCompletedWhenFinalized();
    }
}