using System.Net;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Models;
using SearchService.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

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


