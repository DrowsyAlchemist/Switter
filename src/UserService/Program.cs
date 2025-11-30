using AuthService.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UserService.Consumers;
using UserService.Data;
using UserService.HealthChecks;
using UserService.Interfaces;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;
using UserService.Interfaces.Queries;
using UserService.Models;
using UserService.Services;
using UserService.Services.Commands;
using UserService.Services.Decorators;
using UserService.Services.Infrastructure;
using UserService.Services.Queries;

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
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Repositories
builder.Services.AddScoped<IProfilesRepository, ProfilesRepository>();
builder.Services.AddScoped<IFollowRepository, FollowsRepository>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AllowNullCollections = true;
    cfg.AllowNullDestinationValues = false;
}, typeof(Program).Assembly);

// Services

builder.Services.AddScoped<IUserRelationshipService, UserRelationshipService>();

// ProfileService
builder.Services.AddScoped<ProfileCommands>();
builder.Services.AddScoped<IProfileCommands>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<ProfileCommands>();
    var profileCommandsCached = new ProfileCommandsCached(
        profileCommands: baseService,
        redisService: serviceProvider.GetRequiredService<IRedisService>()
        );
    return profileCommandsCached;
});

builder.Services.Configure<UserServiceOptions>(builder.Configuration.GetSection("UserServiceOptions"));
builder.Services.AddScoped<ProfileQueries>();
builder.Services.AddScoped<IProfileQueries>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<ProfileQueries>();
    var profileQueriesCached = new ProfileQueriesCached(
        profileQueries: baseService,
        redisService: serviceProvider.GetRequiredService<IRedisService>(),
        options: serviceProvider.GetRequiredService<IOptions<UserServiceOptions>>(),
        logger: serviceProvider.GetRequiredService<ILogger<ProfileQueriesCached>>()
        );
    var profileQueriesWithRelationship = new ProfileQueriesWithRelationship(
        profileQueries: profileQueriesCached,
        userRelationshipService: serviceProvider.GetRequiredService<IUserRelationshipService>()
        );
    return profileQueriesWithRelationship;
});

// FollowService
builder.Services.AddScoped<IFollowQueries, FollowQueries>();

builder.Services.AddScoped<FollowCommands>();
builder.Services.AddScoped<IFollowCommands>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<FollowCommands>();

    var serviceWithCounter = new FollowCommandsCounter(
        followCommands: baseService,
        profilesRepository: serviceProvider.GetRequiredService<IProfilesRepository>(),
        redis: serviceProvider.GetRequiredService<IRedisService>()
        );
    var serviceWithKafka = new FollowCommandsWithKafka(
        followCommands: serviceWithCounter,
        kafkaProducer: serviceProvider.GetRequiredService<IKafkaProducer>()
        );
    return serviceWithCounter;
});

// BlockService
builder.Services.AddScoped<IBlockQueries, BlockQueries>();

builder.Services.AddScoped<IBlocker, Blocker>();
builder.Services.AddScoped<BlockCommands>();
builder.Services.AddScoped<IBlockCommands>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<BlockCommands>();

    var blockWithKafka = new BlockCommandsWithKafka(
        blockCommands: baseService,
        kafkaProducer: serviceProvider.GetRequiredService<IKafkaProducer>()
        );
    return blockWithKafka;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL")!)
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!)
    .AddCheck<DatabaseHealthCheck>("Database")
    .AddCheck<ProfileServiceHealthCheck>("ProfileService")
    .AddCheck<FollowServiceHealthCheck>("FollowService")
    .AddCheck<BlockServiceHealthCheck>("BlockService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
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