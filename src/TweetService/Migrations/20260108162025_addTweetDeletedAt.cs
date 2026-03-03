using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TweetService.Migrations
{
    /// <inheritdoc />
    public partial class addTweetDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "tweet_service",
                table: "Tweets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "tweet_service",
                table: "Tweets");
        }
    }
}
