using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Profiles_BlockedId",
                schema: "user_service",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Profiles_BlockerId",
                schema: "user_service",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Profiles_FolloweeId",
                schema: "user_service",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Profiles_FollowerId",
                schema: "user_service",
                table: "Follows");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Profiles_BlockedId",
                schema: "user_service",
                table: "Blocks",
                column: "BlockedId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Profiles_BlockerId",
                schema: "user_service",
                table: "Blocks",
                column: "BlockerId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Profiles_FolloweeId",
                schema: "user_service",
                table: "Follows",
                column: "FolloweeId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Profiles_FollowerId",
                schema: "user_service",
                table: "Follows",
                column: "FollowerId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Profiles_BlockedId",
                schema: "user_service",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Profiles_BlockerId",
                schema: "user_service",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Profiles_FolloweeId",
                schema: "user_service",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Profiles_FollowerId",
                schema: "user_service",
                table: "Follows");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Profiles_BlockedId",
                schema: "user_service",
                table: "Blocks",
                column: "BlockedId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Profiles_BlockerId",
                schema: "user_service",
                table: "Blocks",
                column: "BlockerId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Profiles_FolloweeId",
                schema: "user_service",
                table: "Follows",
                column: "FolloweeId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Profiles_FollowerId",
                schema: "user_service",
                table: "Follows",
                column: "FollowerId",
                principalSchema: "user_service",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
