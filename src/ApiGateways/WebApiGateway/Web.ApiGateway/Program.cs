using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Web.ApiGateway.Services;
using Web.ApiGateway.Services.Interfaces;
using Web.ApiGateway.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Web.ApiGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());
builder.Services.AddHealthChecksUI().AddInMemoryStorage();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web.ApiGateway", Version = "v1" });
});
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.AddOcelot().AddConsul();
ConfigureHttpClient(builder.Services);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder.SetIsOriginAllowed((host) => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<HttpClientDelegatingHandler>();

// Configure the HTTP request pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web.ApiGateway v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");
app.MapHealthChecksUI();

app.UseOcelot().Wait();

app.Run();

void ConfigureHttpClient(IServiceCollection services)
{
    services.AddHttpClient("basket", c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["urls:basket"]);
    })
    .AddHttpMessageHandler<HttpClientDelegatingHandler>();

    services.AddHttpClient("catalog", c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["urls:catalog"]);
    })
    .AddHttpMessageHandler<HttpClientDelegatingHandler>();
}
