using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using PaymentService.Api.IntegrationEvents.EventHandlers;
using PaymentService.Api.IntegrationEvents.Events;
using Microsoft.AspNetCore.HttpsPolicy;

var builder = WebApplication.CreateBuilder(args);

// Environment configuration
string env = builder.Environment.EnvironmentName;

// Configuration setup
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Configurations/appsettings.json", optional: false)
    .AddJsonFile($"Configurations/appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables();

// Serilog Configuration
var serilogConfiguration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Configurations/serilog.json", optional: false)
    .AddJsonFile($"Configurations/serilog.{env}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(serilogConfiguration)
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.AddDebug();
});
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService.Api", Version = "v1" });
});
builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var config = new EventBusConfig
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "PaymentService",
        EventBusType = EventBusType.RabbitMQ,
        Connection = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 15672,
            UserName = "guest",
            Password = "guest"
        }
    };

    return EventBusFactory.Create(config, sp);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService.Api v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

IEventBus eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

app.Run();
