using FeedService.Interfaces.Infrastructure;
using FeedService.Models.Options;
using FeedService.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClients
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();
builder.Services.AddHttpClient<ITweetServiceClient, TweetServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://your-tweet-service.com/");//////
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://your-user-service.com/");//////
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});


// Options
builder.Configuration.AddJsonFile("Configuration/KafkaConfig.json");
builder.Services.Configure<KafkaOptions>(builder.Configuration);

builder.Configuration.AddJsonFile("Configuration/FeedConfig.json");
builder.Services.Configure<FeedOptions>(builder.Configuration);



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();