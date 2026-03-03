using AuthService.Data;
using AuthService.HealthChecks;
using AuthService.Interfaces.Auth;
using AuthService.Interfaces.Infrastructure;
using AuthService.Interfaces.Jwt;
using AuthService.Models;
using AuthService.Services.Auth;
using AuthService.Services.Infrastructure;
using AuthService.Services.Jwt;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Data base
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

// Services
builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL")!)
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!)
    .AddCheck<DatabaseHealthCheck>("Database")
    .AddCheck<TokenServiceHealthCheck>("JwtService")
    .AddCheck<AuthHealthCheck>("AuthService");

var app = builder.Build();

// Migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        Console.WriteLine("Attempting database migration...");
        db.Database.Migrate();
        Console.WriteLine("Database migration completed successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration failed: {ex.Message}");
    Console.WriteLine("Continuing without database migration...");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
