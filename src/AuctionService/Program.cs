using AuctionService.Data;
using AuctionService.RequestHelpers;
using MassTransit;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// Adds AutoMapper to the DI container, scanning all assemblies in the current AppDomain
// for AutoMapper configuration. This is needed because AutoMapper is used in the
// AuctionsController to map Auctions to AuctionDTOs, and AutoMapper is resolved
// from the DI container.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});



var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitializeDb(app);
}
catch (System.Exception exception)
{
    Console.WriteLine(exception);
}

app.Run();
