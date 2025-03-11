using Microsoft.EntityFrameworkCore;
using Stanok.Application.Services;
using Stanok.Core.Abstractions;
using Stanok.DataAccess;
using Stanok.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var configuration = builder.Configuration;
Console.WriteLine($"Connection string: {configuration}");

builder.Services.AddDbContext<StanokDbContext>(
    options =>
    {
        options.UseNpgsql(configuration.GetConnectionString(nameof(StanokDbContext)));
    });

builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IDeliveriesRepository, DeliveriesRepository>();

builder.Services.AddScoped<IStanokService, StanokService>();
builder.Services.AddScoped<IStanoksRepository, StanoksRepository>();

builder.Services.AddHostedService<DeliveryTimeoutService>()
    .AddScoped<IDeliveryTimeoutService, DeliveryTimeoutService>();  


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }