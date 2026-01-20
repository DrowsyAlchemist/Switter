using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TweetService.Consumers;
using TweetService.Data;
using TweetService.HealthChecks;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Services;
using TweetService.Services.Decorators;
using TweetService.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));
builder.Services.AddScoped<IRedisService, RedisService>();

// Kafka 
builder.Services.AddHostedService<UserEventsConsumer>();
builder.Services.AddHostedService<TweetEventsConsumer>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// Database
builder.Services.AddDbContext<TweetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")), ServiceLifetime.Scoped);
builder.Services.AddScoped<ITransactionManager, EfTransactionManager>();

// Repositories
builder.Services.AddScoped<ITweetRepository, TweetRepository>();
builder.Services.AddScoped<ILikesRepository, LikeRepository>();
builder.Services.AddScoped<IHashtagRepository, HashtagRepository>();
builder.Services.AddScoped<ITweetHashtagRepository, TweetHashtagRepository>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AllowNullCollections = true;
    cfg.AllowNullDestinationValues = false;
}, typeof(Program).Assembly);

// Services

builder.Services.AddScoped<IUserTweetRelationship, UserTweetRelationship>();
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>();

// HashtagService
builder.Services.AddScoped<HashtagService>();
builder.Services.AddScoped<IHashtagService>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<HashtagService>();
    var hashtagServiceWithUsage = new HashtagServiceWithUsage(
        hashtagService: baseService,
        redisService: serviceProvider.GetRequiredService<IRedisService>(),
        transactionManager: serviceProvider.GetRequiredService<ITransactionManager>()
        );
    return hashtagServiceWithUsage;
});

// TweetService
builder.Services.AddScoped<TweetCommands>();
builder.Services.AddScoped<ITweetCommands>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<TweetCommands>();
    var tweetWithHashtags = new TweetCommandsWithHashtags(
        tweetCommands: baseService,
        hashtagService: serviceProvider.GetRequiredService<IHashtagService>(),
        transactionManager: serviceProvider.GetRequiredService<ITransactionManager>()
        );
    var tweetCommandsWithKafka = new TweetCommandsWithKafka(
        tweetCommands: tweetWithHashtags,
        kafkaProducer: serviceProvider.GetRequiredService<IKafkaProducer>(),
        logger: serviceProvider.GetRequiredService<ILogger<TweetCommandsWithKafka>>()
        );
    return tweetCommandsWithKafka;
});

builder.Services.AddScoped<ITweetQueries, TweetQueries>();

// LikeService
builder.Services.AddScoped<LikeService>();

builder.Services.AddScoped<ILikeService>(serviceProvider =>
{
    var baseService = serviceProvider.GetRequiredService<LikeService>();

    var likeServiceWithUsage = new LikeServiceWithUsage(
        likeService: baseService,
        redisService: serviceProvider.GetRequiredService<IRedisService>()
        );
    var likeServiceWithKafka = new LikeServiceWithKafka(
        likeService: likeServiceWithUsage,
        kafkaProducer: serviceProvider.GetRequiredService<IKafkaProducer>(),
        logger: serviceProvider.GetRequiredService<ILogger<LikeServiceWithKafka>>()
        );
    return likeServiceWithKafka;
});

// TrendService
builder.Services.AddScoped<ITrendService, TrendService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL")!)
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!)
    .AddCheck<DatabaseHealthCheck>("Database")
    .AddCheck<TweetServiceHealthCheck>("TweetService")
    .AddCheck<LikeServiceHealthCheck>("LikeService");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TweetDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
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