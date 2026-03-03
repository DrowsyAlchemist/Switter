using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.Hubs;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Data;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Models.Options;
using NotificationService.Services;
using NotificationService.Services.EventHandlers.TweetEventHandlers;
using NotificationService.Services.EventHandlers.UserEventHandlers;
using NotificationService.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Options
builder.Configuration.AddJsonFile("Configuration/KafkaConfig.json");
builder.Services.Configure<KafkaOptions>(builder.Configuration);

// Kafka 
builder.Services.AddHostedService<NotificationEventsConsumer>();

// Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")), ServiceLifetime.Scoped);


// Repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();
builder.Services.AddScoped<IWebSocketConnectionRepository, WebSocketConnectionInMemoryRepository>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AllowNullCollections = true;
    cfg.AllowNullDestinationValues = false;
}, typeof(Program).Assembly);

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024;
})
.AddJsonProtocol();

// Services

builder.Services.AddScoped<IWebSocketMessager, WebSocketMessager>();
builder.Services.AddScoped<INotificationDeliveryService, NotificationDeliveryService>();

builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();

builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>();
builder.Services.AddSingleton<INotificationEventsProcessor, NotificationEventsProcessor>();

builder.Services.AddSingleton<LikeSetEventHandler>();
builder.Services.AddSingleton<ReplyEventHandler>();
builder.Services.AddSingleton<RetweetEventHandler>();
builder.Services.AddSingleton<TweetCreatedEventHandler>();
builder.Services.AddSingleton<FollowEventHandler>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL")!);

builder.Services.AddAuthentication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.GetRequiredService<LikeSetEventHandler>();
app.Services.GetRequiredService<ReplyEventHandler>();
app.Services.GetRequiredService<RetweetEventHandler>();
app.Services.GetRequiredService<TweetCreatedEventHandler>();
app.Services.GetRequiredService<FollowEventHandler>();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHealthChecks("/health");

app.Run();
