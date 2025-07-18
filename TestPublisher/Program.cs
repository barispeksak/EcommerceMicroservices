using System;
using System.Threading.Tasks;
using MassTransit;
using Common.Contracts.Commands;

class Program
{
    static async Task Main()
    {
        var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host("rabbitmq", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        });

        await bus.StartAsync();
        Console.WriteLine("Bus started.");

        var cartId        = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var itemIds       = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }; // Create some dummy items

        Console.WriteLine($"CartId:        {cartId}");
        Console.WriteLine($"CorrelationId: {correlationId}");
        Console.WriteLine($"Items:         {string.Join(", ", itemIds)}");

        var message = new FinalStockCheckRequested(cartId, itemIds)
        {
            CorrelationId = correlationId,
            SourceService = "ManualTest"
        };
        
        await bus.Publish(message);
        Console.WriteLine("Message published. Enter to exit.");
        Console.ReadLine();

        await bus.StopAsync();
    }
}