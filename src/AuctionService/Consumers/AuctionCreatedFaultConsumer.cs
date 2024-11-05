using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("---> Consuming faulty creation from Auction Service");

        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType.Equals(typeof(System.ArgumentException)))
        {
            context.Message.Message.Model = "FooBar";


            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine("Not an argument exception - update error dashboard somewhere");
        }
    }
}