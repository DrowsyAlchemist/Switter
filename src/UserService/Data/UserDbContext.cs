using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Block> Blocks { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public UserDbContext() { }

        public async Task<bool> CanConnectAsync()
        {
            return await Database.CanConnectAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("user_service");

            // Profiles
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.DisplayName);

                entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Bio).HasMaxLength(500);
                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(u => u.UpdatedAt).HasColumnType("timestamp with time zone");

                entity.HasMany(u => u.Followers)
                             .WithOne(f => f.Followee)
                             .HasForeignKey(f => f.FolloweeId);

                entity.HasMany(u => u.Following)
                      .WithOne(f => f.Follower)
                      .HasForeignKey(f => f.FollowerId);
            });

            // Follows
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.ToTable("Follows");

                entity.HasIndex(f => new { f.FollowerId, f.FolloweeId }).IsUnique();

                entity.HasOne(f => f.Follower)
                      .WithMany(u => u.Following)
                      .HasForeignKey(f => f.FollowerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Followee)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(f => f.FolloweeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
            });

            // Blocks
            modelBuilder.Entity<Block>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => new { b.BlockerId, b.BlockedId }).IsUnique();
                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");

                entity.HasOne(b => b.Blocker)
                    .WithMany()
                    .HasForeignKey(и => и.BlockerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Blocked)
                      .WithMany()
                      .HasForeignKey(b => b.BlockedId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}