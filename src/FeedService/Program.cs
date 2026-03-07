using FeedService.Consumers;
using FeedService.Data;
using FeedService.HealthChecks;
using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;
using FeedService.Models.Options;
using FeedService.Services;
using FeedService.Services.Infrastructure;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Options
builder.Configuration.AddJsonFile("Configuration/KafkaConfig.json");
builder.Services.Configure<KafkaOptions>(builder.Configuration);

builder.Configuration.AddJsonFile("Configuration/FeedConfig.json");
builder.Services.Configure<FeedOptions>(builder.Configuration);

builder.Configuration.AddJsonFile("Configuration/AppUrls.json");
builder.Services.Configure<AppUrls>(builder.Configuration);

// HttpClients
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();
builder.Services.AddHttpClient<ITweetServiceClient, TweetServiceClient>(client =>
{
    var tweetServiceUrl = builder.Configuration["tweetServiceUrl"] ?? throw new Exception("Tweet service url not found.");
    client.BaseAddress = new Uri(tweetServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>(client =>
{
    var userServiceUrl = builder.Configuration["userServiceUrl"] ?? throw new Exception("User service url not found.");
    client.BaseAddress = new Uri(userServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

// Kafka
builder.Services.AddScoped<FeedEventsConsumer>();

// Repositories
builder.Services.AddScoped<IFeedRepository, RedisFeedRepository>();
builder.Services.AddSingleton<IFollowsRepository, FollowsInMemoryRepository>();

// Services
builder.Services.AddScoped<IFeedScoreCalculator, FeedScoreCalculator>();
builder.Services.AddScoped<IFeedBuilder, FeedBuilder>();
builder.Services.AddScoped<IFeedFiller, FeedFiller>();
builder.Services.AddScoped<IFeedEventProcessor, FeedEventProcessor>();

builder.Services.AddScoped<IFeedService, FeedService.Services.FeedService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!)
    .AddCheck<UserClientHealthCheck>("UserClient")
    .AddCheck<TweetClientHealthCheck>("TweetClient");

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/ping", () =>
{
    return "pong";
});

app.Run();