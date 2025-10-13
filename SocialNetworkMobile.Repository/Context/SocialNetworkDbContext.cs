using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using SocialNetworkMobile.Repository.Models;

namespace SocialNetworkMobile.Repository.Context
{
    public partial class SocialNetworkDbContext : DbContext
    {
        public SocialNetworkDbContext()
        {
        }

        public SocialNetworkDbContext(DbContextOptions<SocialNetworkDbContext> options)
            : base(options)
        {
        }

        // Core tables
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Like> Likes { get; set; }
        public virtual DbSet<Follow> Follows { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<PostTag> PostTags { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<UserSocialProvider> UserSocialProviders { get; set; }

        public static string GetConnectionString(string connectionStringName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string connectionString = config.GetConnectionString(connectionStringName);
            return connectionString ?? throw new InvalidOperationException("Connection string not found.");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(GetConnectionString("DefaultConnection"),
                options => options.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), null)
                                 .CommandTimeout(30))
                             .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_users");
                entity.ToTable("users", "public");
                entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Email).HasColumnName("Email").IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255);
                entity.Property(e => e.FullName).HasColumnName("FullName").HasMaxLength(255);
                entity.Property(e => e.AvatarUrl).HasColumnName("AvatarUrl").HasMaxLength(1024);
                entity.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20);
                entity.Property(e => e.Bio).HasColumnName("Bio").HasMaxLength(500);
                entity.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth").HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                entity.Property(e => e.Location).HasColumnName("Location").HasMaxLength(100);
                entity.Property(e => e.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                entity.Property(e => e.EmailVerifiedAt).HasColumnName("EmailVerifiedAt").HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                entity.Property(e => e.LastLoginAt).HasColumnName("LastLoginAt").HasConversion(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP").HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP").HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });

            // Posts
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_posts");
                entity.ToTable("posts", "public");
                entity.HasIndex(e => e.UserId, "ix_posts_user_id");
                entity.HasIndex(e => e.CreatedAt, "ix_posts_created_at");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Content).HasColumnName("Content").IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(500);
                entity.Property(e => e.VideoUrl).HasColumnName("VideoUrl").HasMaxLength(500);
                entity.Property(e => e.LikeCount).HasColumnName("LikeCount").HasDefaultValue(0);
                entity.Property(e => e.CommentCount).HasColumnName("CommentCount").HasDefaultValue(0);
                entity.Property(e => e.ShareCount).HasColumnName("ShareCount").HasDefaultValue(0);
                entity.Property(e => e.IsPublic).HasColumnName("IsPublic").HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.User).WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_posts_user_id");
            });

            // Comments
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_comments");
                entity.ToTable("comments", "public");
                entity.HasIndex(e => e.PostId, "ix_comments_post_id");
                entity.HasIndex(e => e.UserId, "ix_comments_user_id");
                entity.HasIndex(e => e.ParentCommentId, "ix_comments_parent_comment_id");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.PostId).HasColumnName("PostId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Content).HasColumnName("Content").IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ParentCommentId).HasColumnName("ParentCommentId");
                entity.Property(e => e.LikeCount).HasColumnName("LikeCount").HasDefaultValue(0);
                entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("fk_comments_post_id");
                entity.HasOne(d => d.User).WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_comments_user_id");
                entity.HasOne(d => d.ParentComment).WithMany(p => p.Replies)
                    .HasForeignKey(d => d.ParentCommentId)
                    .HasConstraintName("fk_comments_parent_comment_id");
            });

            // Likes
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_likes");
                entity.ToTable("likes", "public");
                entity.HasIndex(e => e.UserId, "ix_likes_user_id");
                entity.HasIndex(e => e.PostId, "ix_likes_post_id");
                entity.HasIndex(e => e.CommentId, "ix_likes_comment_id");
                entity.HasIndex(e => new { e.UserId, e.PostId }, "uq_likes_user_post").IsUnique();
                entity.HasIndex(e => new { e.UserId, e.CommentId }, "uq_likes_user_comment").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.PostId).HasColumnName("PostId");
                entity.Property(e => e.CommentId).HasColumnName("CommentId");
                entity.Property(e => e.LikeType).HasColumnName("LikeType").IsRequired().HasMaxLength(20).HasDefaultValue("LIKE");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.User).WithMany(p => p.Likes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_likes_user_id");
                entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("fk_likes_post_id");
                entity.HasOne(d => d.Comment).WithMany(p => p.Likes)
                    .HasForeignKey(d => d.CommentId)
                    .HasConstraintName("fk_likes_comment_id");
            });

            // Follows
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_follows");
                entity.ToTable("follows", "public");
                entity.HasIndex(e => e.FollowerId, "ix_follows_follower_id");
                entity.HasIndex(e => e.FollowingId, "ix_follows_following_id");
                entity.HasIndex(e => new { e.FollowerId, e.FollowingId }, "uq_follows_follower_following").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.FollowerId).HasColumnName("FollowerId");
                entity.Property(e => e.FollowingId).HasColumnName("FollowingId");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.Follower).WithMany(p => p.Following)
                    .HasForeignKey(d => d.FollowerId)
                    .HasConstraintName("fk_follows_follower_id");
                entity.HasOne(d => d.Following).WithMany(p => p.Followers)
                    .HasForeignKey(d => d.FollowingId)
                    .HasConstraintName("fk_follows_following_id");
            });

            // Tags
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_tags");
                entity.ToTable("tags", "public");
                entity.HasIndex(e => e.Name, "uq_tags_name").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("Name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("Description").HasMaxLength(500);
                entity.Property(e => e.UsageCount).HasColumnName("UsageCount").HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Post Tags
            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_post_tags");
                entity.ToTable("post_tags", "public");
                entity.HasIndex(e => e.PostId, "ix_post_tags_post_id");
                entity.HasIndex(e => e.TagId, "ix_post_tags_tag_id");
                entity.HasIndex(e => new { e.PostId, e.TagId }, "uq_post_tags_post_tag").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.PostId).HasColumnName("PostId");
                entity.Property(e => e.TagId).HasColumnName("TagId");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.Post).WithMany(p => p.PostTags)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("fk_post_tags_post_id");
                entity.HasOne(d => d.Tag).WithMany(p => p.PostTags)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("fk_post_tags_tag_id");
            });

            // Notifications
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_notifications");
                entity.ToTable("notifications", "public");
                entity.HasIndex(e => e.UserId, "ix_notifications_user_id");
                entity.HasIndex(e => e.FromUserId, "ix_notifications_from_user_id");
                entity.HasIndex(e => e.Type, "ix_notifications_type");
                entity.HasIndex(e => e.IsRead, "ix_notifications_is_read");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.FromUserId).HasColumnName("FromUserId");
                entity.Property(e => e.Type).HasColumnName("Type").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).HasColumnName("Title").HasMaxLength(500);
                entity.Property(e => e.Message).HasColumnName("Message").HasMaxLength(1000);
                entity.Property(e => e.PostId).HasColumnName("PostId");
                entity.Property(e => e.CommentId).HasColumnName("CommentId");
                entity.Property(e => e.IsRead).HasColumnName("IsRead").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_notifications_user_id");
                entity.HasOne(d => d.FromUser).WithMany(p => p.SentNotifications)
                    .HasForeignKey(d => d.FromUserId)
                    .HasConstraintName("fk_notifications_from_user_id");
            });

            // User Social Providers
            modelBuilder.Entity<UserSocialProvider>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_user_social_providers");
                entity.ToTable("user_social_providers", "public");
                entity.HasIndex(e => e.UserId, "ix_user_social_providers_user_id");
                entity.HasIndex(e => new { e.ProviderName, e.ProviderId }, "uq_user_social_providers_provider").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.ProviderName).HasColumnName("ProviderName").IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProviderId).HasColumnName("ProviderId").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(255);
                entity.Property(e => e.ProfileUrl).HasColumnName("ProfileUrl").HasMaxLength(2048);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.User).WithMany(p => p.UserSocialProviders)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_user_social_providers_user_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
