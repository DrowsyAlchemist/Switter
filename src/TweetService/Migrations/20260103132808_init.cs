using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TweetService.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tweet_service");

            migrationBuilder.CreateTable(
                name: "Hashtags",
                schema: "tweet_service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    FirstUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashtags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tweets",
                schema: "tweet_service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorDisplayName = table.Column<string>(type: "text", nullable: false),
                    AuthorAvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                    ParentTweetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    LikesCount = table.Column<int>(type: "integer", nullable: false),
                    RetweetsCount = table.Column<int>(type: "integer", nullable: false),
                    RepliesCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tweets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tweets_Tweets_ParentTweetId",
                        column: x => x.ParentTweetId,
                        principalSchema: "tweet_service",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                schema: "tweet_service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TweetId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "tweet_service",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetHashtags",
                schema: "tweet_service",
                columns: table => new
                {
                    TweetId = table.Column<Guid>(type: "uuid", nullable: false),
                    HashtagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetHashtags", x => new { x.TweetId, x.HashtagId });
                    table.ForeignKey(
                        name: "FK_TweetHashtags_Hashtags_HashtagId",
                        column: x => x.HashtagId,
                        principalSchema: "tweet_service",
                        principalTable: "Hashtags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TweetHashtags_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "tweet_service",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_Tag",
                schema: "tweet_service",
                table: "Hashtags",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_TweetId_UserId",
                schema: "tweet_service",
                table: "Likes",
                columns: new[] { "TweetId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TweetHashtags_HashtagId",
                schema: "tweet_service",
                table: "TweetHashtags",
                column: "HashtagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_AuthorId",
                schema: "tweet_service",
                table: "Tweets",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_CreatedAt",
                schema: "tweet_service",
                table: "Tweets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_ParentTweetId",
                schema: "tweet_service",
                table: "Tweets",
                column: "ParentTweetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes",
                schema: "tweet_service");

            migrationBuilder.DropTable(
                name: "TweetHashtags",
                schema: "tweet_service");

            migrationBuilder.DropTable(
                name: "Hashtags",
                schema: "tweet_service");

            migrationBuilder.DropTable(
                name: "Tweets",
                schema: "tweet_service");
        }
    }
}
