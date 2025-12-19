using Microsoft.EntityFrameworkCore;
using TweetService.Models;

namespace TweetService.Data
{
    public class TweetDbContext : DbContext
    {
        public TweetDbContext(DbContextOptions<TweetDbContext> options) : base(options) { }

        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<TweetHashtag> TweetHashtags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("tweet_service");

            // Tweet
            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.HasIndex(t => t.AuthorId);
                entity.HasIndex(t => t.CreatedAt);
                entity.HasIndex(t => t.ParentTweetId);

                entity.Property(t => t.Content).IsRequired().HasMaxLength(280);
                entity.Property(t => t.CreatedAt).HasColumnType("timestamp with time zone");

                entity.HasOne(t => t.ParentTweet)
                      .WithMany(t => t.Replies)
                      .HasForeignKey(t => t.ParentTweetId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Like
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);

                entity.HasIndex(l => new { l.TweetId, l.UserId }).IsUnique();

                entity.HasOne(l => l.Tweet)
                      .WithMany(t => t.Likes)
                      .HasForeignKey(l => l.TweetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Hashtag
            modelBuilder.Entity<Hashtag>(entity =>
            {
                entity.HasKey(h => h.Id);

                entity.HasIndex(h => h.Tag).IsUnique();
                entity.Property(h => h.Tag).IsRequired().HasMaxLength(50);
            });

            // TweetHashtag
            modelBuilder.Entity<TweetHashtag>(entity =>
            {
                entity.HasKey(th => new { th.TweetId, th.HashtagId });

                entity.HasOne(th => th.Tweet)
                      .WithMany()
                      .HasForeignKey(th => th.TweetId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(th => th.Hashtag)
                      .WithMany(h => h.TweetHashtags)
                      .HasForeignKey(th => th.HashtagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
