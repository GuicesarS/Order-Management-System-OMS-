using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Configurations;
using OrderManagement.API.Configurations.CustomMapping;
using OrderManagement.Application;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);

ExpressMappingConfig.RegisterMappings();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICustomMapper, CustomMapper>();

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Database configuration
builder.Services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

