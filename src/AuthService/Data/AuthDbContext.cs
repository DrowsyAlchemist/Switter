using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("auth_service");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(u => u.UpdatedAt).HasColumnType("timestamp with time zone");
            });
            //modelBuilder.Entity<RefreshToken>(entity =>
            //{
            //    entity.HasKey(rt => rt.Id);
            //    entity.HasIndex(rt => rt.Token).IsUnique();

            //    entity.HasOne(rt => rt.User)
            //    .WithMany(u => u.RefreshTokens)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);
            //});
            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> CanConnectAsync()
        {
            return await Database.CanConnectAsync();
        }
    }
}
