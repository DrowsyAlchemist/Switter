using AuthService.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UserService.Consumers;
using UserService.Data;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;
using UserService.Services;
using UserService.Services.Decorators;
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
//builder.Services.AddHostedService<AuthEventsConsumer>();
//builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

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
});

// Services

// ProfileService
builder.Services.AddScoped<IUserRelationshipService, ProfileRelationshipService>();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<IUserProfileService>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<UserProfileService>();
    var cachedService = new CachedProfileService(
        profileService: baseService,
        redisService: serviceProvider.GetRequiredService<IRedisService>(),
        logger: serviceProvider.GetRequiredService<ILogger<CachedProfileService>>()
    );
    return cachedService;
});

// FollowService
builder.Services.AddScoped<FollowService>();
builder.Services.AddScoped<IFollowService>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<FollowService>();

    var serviceWithCounter = new FollowWithCounterService(
        followService: baseService,
        profilesRepository: serviceProvider.GetRequiredService<IProfilesRepository>(),
        redis: serviceProvider.GetRequiredService<IRedisService>()
        );
    //var serviceWithKafka = new FollowServiceWithKafka(
    //    followService: serviceWithCounter,
    //    kafkaProducer: serviceProvider.GetRequiredService<IKafkaProducer>()
    //    );
    return serviceWithCounter;
});

// BlockService
builder.Services.AddScoped<Blocker>();
builder.Services.AddScoped<BlockService>();
builder.Services.AddScoped<IBlockService>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<BlockService>();

    //var blockWithKafka = new BlockServiceWithKafka(
    //    blockService: baseService,
    //    kafkaProducer: serviceProvider.GetRequiredService<IKafkaProducer>()
    //    );
    return baseService;
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