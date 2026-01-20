using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Configurations;
using OrderManagement.API.Handlers;
using OrderManagement.Application;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Data;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

ExpressMappingConfig.RegisterMappings();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Add OpenAPI/Swagger services
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext() 
    .WriteTo.Console()
    .WriteTo.File("logs/ordermanagementapi-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Add Custom Mapper service
builder.Services.AddScoped<ICustomMapper, CustomMapper>();

builder.Services.AddControllers();

// Add Validation DTO services
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

//Add Exception Handling Middleware
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Application & Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure();       

// Database configuration
builder.Services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

