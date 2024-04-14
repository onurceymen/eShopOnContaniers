using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using PaymentService.Api.IntegrationEvents.EventHandlers;
using PaymentService.Api.IntegrationEvents.Events;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapýlandýrmasý
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext());

// Servislerin eklenmesi
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PaymentService.Api", Version = "v1" });
});
builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var eventBusConfig = new EventBusConfig
    {
        ConnectionRetryCount = 5,
        EventNameSuffix = "IntegrationEvent",
        SubscriberClientAppName = "PaymentService",
        EventBusType = EventBusType.RabbitMQ,
        Connection = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672, // Not 15672, that's the management port
            UserName = "guest",
            Password = "guest"
        }
    };
    return EventBusFactory.Create(eventBusConfig, sp);
});

var app = builder.Build();

// HTTP istek iþleyicilerinin yapýlandýrýlmasý
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

// EventBus aboneliklerinin yapýlandýrýlmasý
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

app.Run();