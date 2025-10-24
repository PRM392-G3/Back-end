using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SocialNetworkMobile.Repository.Migrations
{
    /// <inheritdoc />
    public partial class _20251024100630_InitialDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EmailVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "follows",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerId = table.Column<int>(type: "integer", nullable: false),
                    FollowingId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_follows", x => x.Id);
                    table.ForeignKey(
                        name: "fk_follows_follower_id",
                        column: x => x.FollowerId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_follows_following_id",
                        column: x => x.FollowingId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FromUserId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PostId = table.Column<int>(type: "integer", nullable: true),
                    CommentId = table.Column<int>(type: "integer", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "fk_notifications_from_user_id",
                        column: x => x.FromUserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "fk_notifications_user_id",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CommentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShareCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.Id);
                    table.ForeignKey(
                        name: "fk_posts_user_id",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_social_providers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProfileUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_social_providers", x => x.Id);
                    table.ForeignKey(
                        name: "fk_user_social_providers_user_id",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    LikeCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.Id);
                    table.ForeignKey(
                        name: "fk_comments_parent_comment_id",
                        column: x => x.ParentCommentId,
                        principalSchema: "public",
                        principalTable: "comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "fk_comments_post_id",
                        column: x => x.PostId,
                        principalSchema: "public",
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comments_user_id",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_tags",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_tags", x => x.Id);
                    table.ForeignKey(
                        name: "fk_post_tags_post_id",
                        column: x => x.PostId,
                        principalSchema: "public",
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_tags_tag_id",
                        column: x => x.TagId,
                        principalSchema: "public",
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "likes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PostId = table.Column<int>(type: "integer", nullable: true),
                    CommentId = table.Column<int>(type: "integer", nullable: true),
                    LikeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "LIKE"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_likes", x => x.Id);
                    table.ForeignKey(
                        name: "fk_likes_comment_id",
                        column: x => x.CommentId,
                        principalSchema: "public",
                        principalTable: "comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "fk_likes_post_id",
                        column: x => x.PostId,
                        principalSchema: "public",
                        principalTable: "posts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "fk_likes_user_id",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comments_parent_comment_id",
                schema: "public",
                table: "comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "ix_comments_post_id",
                schema: "public",
                table: "comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id",
                schema: "public",
                table: "comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_follows_follower_id",
                schema: "public",
                table: "follows",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "ix_follows_following_id",
                schema: "public",
                table: "follows",
                column: "FollowingId");

            migrationBuilder.CreateIndex(
                name: "uq_follows_follower_following",
                schema: "public",
                table: "follows",
                columns: new[] { "FollowerId", "FollowingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_likes_comment_id",
                schema: "public",
                table: "likes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "ix_likes_post_id",
                schema: "public",
                table: "likes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "ix_likes_user_id",
                schema: "public",
                table: "likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_likes_user_comment",
                schema: "public",
                table: "likes",
                columns: new[] { "UserId", "CommentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_likes_user_post",
                schema: "public",
                table: "likes",
                columns: new[] { "UserId", "PostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_from_user_id",
                schema: "public",
                table: "notifications",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_is_read",
                schema: "public",
                table: "notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_type",
                schema: "public",
                table: "notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                schema: "public",
                table: "notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_post_id",
                schema: "public",
                table: "post_tags",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_tag_id",
                schema: "public",
                table: "post_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "uq_post_tags_post_tag",
                schema: "public",
                table: "post_tags",
                columns: new[] { "PostId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_created_at",
                schema: "public",
                table: "posts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_posts_user_id",
                schema: "public",
                table: "posts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_tags_name",
                schema: "public",
                table: "tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_social_providers_user_id",
                schema: "public",
                table: "user_social_providers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_user_social_providers_provider",
                schema: "public",
                table: "user_social_providers",
                columns: new[] { "ProviderName", "ProviderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                schema: "public",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "follows",
                schema: "public");

            migrationBuilder.DropTable(
                name: "likes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "post_tags",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_social_providers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "comments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "public");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}

