using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Serilog;
using System;
using System.IO;
using IdentityService.Api.Application.Services;
using IdentityService.Api.Extensions.Registration;
using Microsoft.Extensions.Diagnostics.HealthChecks;


var builder = WebApplication.CreateBuilder(args);

// Environment configuration
string env = builder.Environment.EnvironmentName;

// Configuration setup
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"Configurations/appsettings.json", optional: false)
    .AddJsonFile($"Configurations/appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Serilog Configuration
IConfiguration serilogConfiguration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Configurations/serilog.json", optional: false)
    .AddJsonFile($"Configurations/serilog.{env}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(serilogConfiguration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddScoped<IIdentityServices, IdentityServices>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService.Api", Version = "v1" });
});
builder.Services.ConfigureConsul(configuration);
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityService.Api v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Using top-level route registrations
app.MapControllers();
app.MapHealthChecks("/hc", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self"),
    ResponseWriter = async (context, healthReport) =>
    {
        await Task.CompletedTask;
    }
});

app.RegisterWithConsul(app.Lifetime, configuration);

app.Run();

Log.Logger.Information("Application is Running....");
