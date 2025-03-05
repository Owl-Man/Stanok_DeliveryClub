using Microsoft.EntityFrameworkCore;
using Stanok.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var configuration = builder.Configuration;

builder.Services.AddDbContext<StanokDbContext>(
    options =>
    {
        options.UseNpgsql(configuration.GetConnectionString(nameof(StanokDbContext)));
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
