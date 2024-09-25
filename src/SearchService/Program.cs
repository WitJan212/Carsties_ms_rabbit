using System.Net;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Models;
using SearchService.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Adds MassTransit to the DI container. MassTransit is a message bus library
// that allows us to send messages between services. In this case, we are
// configuring MassTransit to use RabbitMQ as the message broker.
//
// The UsingRabbitMq method is a convenience method that allows us to
// configure MassTransit to use RabbitMQ. The method takes a callback that
// is passed an instance of IRabbitMqBusFactoryConfigurator, which is used
// to configure the MassTransit bus.
//
// The ConfigureEndpoints method is a convenience method that allows us to
// configure MassTransit to automatically configure endpoints for all
// message types that are defined in the assembly. This means that we don't
// have to manually configure each endpoint - MassTransit will do it for
// us. The method takes an instance of IRegistrationContext, which is used
// to get the DI container and the assembly that contains the message types.
builder.Services.AddMassTransit(config =>
{
    // Configure the consumers
    // This method is a convenience method that allows us to configure MassTransit to use
    // an existing instance of IRegistrationContext. This is useful when we want to
    // configure MassTransit to use a different assembly than the one that is currently
    // executing. The method takes an instance of IRegistrationContext as a parameter.
    config.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    
    config.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// Call InitializeDb when the application starts
app.Lifetime.ApplicationStarted.Register(async ()=>
{
    try
    {
        await DbInitializer.InitializeDb(app);
    }
    catch (System.Exception exception)
    {
        Console.WriteLine(exception);
        throw;
    }
}); 

app.Run();


/// <summary>
/// Provides a default retry policy for HTTP requests.
/// </summary>
/// <remarks>
/// The policy is configured to retry indefinitely with a 3 second delay between retries.
/// It will retry on transient HTTP errors and on 404 responses.
/// </remarks>
/// <returns>The HTTP request policy.</returns>
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(message => message.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));


