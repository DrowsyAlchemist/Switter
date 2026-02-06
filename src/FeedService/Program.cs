using FeedService.Consumers;
using FeedService.Data;
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

// HttpClients
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();
builder.Services.AddHttpClient<ITweetServiceClient, TweetServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:80");//////
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:80");//////
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
// ...

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
//app.MapHealthChecks("/health");

app.Run();