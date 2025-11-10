using AuthService.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UserService.Consumers;
using UserService.Data;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;
using UserService.Services;
using UserService.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));
builder.Services.AddScoped<IRedisService, RedisService>();

// Kafka 
builder.Services.AddHostedService<AuthEventsConsumer>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

// Database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Services
builder.Services.AddScoped<IProfilesRepository, ProfilesRepository>();
builder.Services.AddScoped<IFollowRepository, FollowsRepository>();
builder.Services.AddScoped<IFollowChecker, FollowsRepository>();
builder.Services.AddScoped<IFollowersCounter, FollowersCounter>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();


// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AllowNullCollections = true;
    cfg.AllowNullDestinationValues = false;
});

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
//    db.Database.Migrate();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();