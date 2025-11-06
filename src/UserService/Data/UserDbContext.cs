using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Block> Blocks { get; set; }

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

                entity.HasMany(e => e.Followers)
               .WithMany(e => e.Following)
               .UsingEntity<Dictionary<string, object>>(
                   "Follows",
                   j => j.HasOne<UserProfile>().WithMany().HasForeignKey("FollowerId"),
                   j => j.HasOne<UserProfile>().WithMany().HasForeignKey("FolloweeId"),
                   j =>
                   {
                       j.HasKey("FollowerId", "FolloweeId");
                       j.ToTable("Follows");
                   });
            });

            // Follows
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.HasIndex(f => new { f.FollowerId, f.FolloweeId }).IsUnique();

                entity.HasOne(f => f.Follower)
                      .WithMany()
                      .HasForeignKey(f => f.FollowerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Followee)
                      .WithMany()
                      .HasForeignKey(f => f.FolloweeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
            });

            // Blocks
            modelBuilder.Entity<Block>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => new { b.BlockerId, b.BlockedId }).IsUnique();
                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
            });
        }
    }
}