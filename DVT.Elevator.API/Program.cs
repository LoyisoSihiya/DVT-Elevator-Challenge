using DVT.Elevator.Domain.Interfaces;
using DVT.Elevator.Services.Mappings;
using DVT.Elevator.Services.Services;
using DVT.Elevator.Services.Strategies;
using DVT.Elevator.Infrastructure.Data;
using DVT.Elevator.Infrastructure.Hubs;
using DVT.Elevator.Infrastructure.Repositories;
using DVT.Elevator.Infrastructure.Services;
using DVT.Elevator.API.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings in JSON responses
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Database Configuration
builder.Services.AddDbContext<ElevatorDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("DVT.Elevator.Infrastructure")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Repository Pattern & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IElevatorService, ElevatorService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IPassengerRequestService, PassengerRequestService>();

// Dispatch Strategy
builder.Services.AddScoped<IElevatorDispatchStrategy, NearestElevatorDispatchStrategy>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IElevatorHubService, ElevatorHubService>();

// Background Services
if (builder.Configuration.GetValue<bool>("ElevatorSimulation:EnableSimulation", true))
{
    builder.Services.AddHostedService<ElevatorSimulationService>();
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ElevatorDbContext>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DVT Elevator Challenge API",
        Version = "v1",
        Description = "Production-ready ASP.NET Core Web API for elevator simulation system",
        Contact = new OpenApiContact
        {
            Name = "DVT",
            Email = "info@dvt.co.za"
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DVT Elevator API v1");
    c.RoutePrefix = "swagger"; // Swagger at /swagger
});

// Global Exception Handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// SignalR Hub
app.MapHub<ElevatorHub>("/hubs/elevator");

// Health Check Endpoint
app.MapHealthChecks("/health");

app.Run();
