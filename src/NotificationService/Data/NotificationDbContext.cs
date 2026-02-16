using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotificationSettings> UserNotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("notification_service");

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.CreatedAt);
                entity.HasIndex(n => new { n.UserId, n.Status });

                entity.Property(n => n.Title).IsRequired().HasMaxLength(100);
                entity.Property(n => n.Message).HasMaxLength(500);
                entity.Property(n => n.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(n => n.ReadAt).HasColumnType("timestamp with time zone");
                entity.Property(n => n.SentAt).HasColumnType("timestamp with time zone");
            });

            // UserNotificationSettings
            modelBuilder.Entity<UserNotificationSettings>(entity =>
            {
                entity.HasKey(s => s.UserId);
                entity.Property(s => s.UpdatedAt).HasColumnType("timestamp with time zone");
            });
        }
    }
}
