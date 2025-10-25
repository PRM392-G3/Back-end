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
        public virtual DbSet<Friendship> Friendships { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupMember> GroupMembers { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<PostTag> PostTags { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<UserSocialProvider> UserSocialProviders { get; set; }
        public virtual DbSet<Share> Shares { get; set; }
        public virtual DbSet<Reel> Reels { get; set; }
        public virtual DbSet<ReelMusic> ReelMusics { get; set; }

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
                entity.Property(e => e.CoverImageUrl).HasColumnName("CoverImageUrl").HasMaxLength(1024);
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
                entity.HasIndex(e => e.ReelId, "ix_comments_reel_id");
                entity.HasIndex(e => e.UserId, "ix_comments_user_id");
                entity.HasIndex(e => e.ParentCommentId, "ix_comments_parent_comment_id");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.PostId).HasColumnName("PostId");
                entity.Property(e => e.ReelId).HasColumnName("ReelId");
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
                entity.HasOne(d => d.Reel).WithMany(r => r.Comments)
                    .HasForeignKey(d => d.ReelId)
                    .HasConstraintName("fk_comments_reel_id");
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
                entity.Property(e => e.ReelId).HasColumnName("ReelId");
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
                entity.HasOne(d => d.Reel).WithMany()
                    .HasForeignKey(d => d.ReelId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_likes_reel_id");
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

            // Friendships
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_friendships");
                entity.ToTable("friendships", "public");
                entity.HasIndex(e => e.RequesterId, "ix_friendships_requester_id");
                entity.HasIndex(e => e.ReceiverId, "ix_friendships_receiver_id");
                entity.HasIndex(e => e.Status, "ix_friendships_status");
                entity.HasIndex(e => new { e.RequesterId, e.ReceiverId }, "uq_friendships_requester_receiver").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.RequesterId).HasColumnName("RequesterId");
                entity.Property(e => e.ReceiverId).HasColumnName("ReceiverId");
                entity.Property(e => e.Status).HasColumnName("Status").IsRequired().HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.RequestedAt).HasColumnName("RequestedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.RespondedAt).HasColumnName("RespondedAt");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.Requester).WithMany(p => p.SentFriendRequests)
                    .HasForeignKey(d => d.RequesterId)
                    .HasConstraintName("fk_friendships_requester_id");
                entity.HasOne(d => d.Receiver).WithMany(p => p.ReceivedFriendRequests)
                    .HasForeignKey(d => d.ReceiverId)
                    .HasConstraintName("fk_friendships_receiver_id");
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

            // Shares
            modelBuilder.Entity<Share>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_shares");
                entity.ToTable("shares", "public");
                entity.HasIndex(e => e.UserId, "ix_shares_user_id");
                entity.HasIndex(e => e.PostId, "ix_shares_post_id");
                entity.HasIndex(e => new { e.UserId, e.PostId }, "uq_shares_user_post").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.PostId).HasColumnName("PostId");
                entity.Property(e => e.Caption).HasColumnName("Caption").HasMaxLength(500);
                entity.Property(e => e.IsPublic).HasColumnName("IsPublic").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.User).WithMany(p => p.Shares)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_shares_user_id");
                entity.HasOne(d => d.Post).WithMany(p => p.Shares)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("fk_shares_post_id");
            });

            // Reels
            modelBuilder.Entity<Reel>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_reels");
                entity.ToTable("reels", "public");
                entity.HasIndex(e => e.UserId, "ix_reels_user_id");
                entity.HasIndex(e => e.MusicId, "ix_reels_music_id");
                entity.HasIndex(e => e.CreatedAt, "ix_reels_created_at");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.VideoUrl).HasColumnName("VideoUrl").IsRequired();
                entity.Property(e => e.VideoFileName).HasColumnName("VideoFileName");
                entity.Property(e => e.Caption).HasColumnName("Caption").HasMaxLength(300);
                entity.Property(e => e.MusicId).HasColumnName("MusicId");
                entity.Property(e => e.MusicUrl).HasColumnName("MusicUrl");
                entity.Property(e => e.MusicFileName).HasColumnName("MusicFileName");
                entity.Property(e => e.MusicTitle).HasColumnName("MusicTitle");
                entity.Property(e => e.MusicArtist).HasColumnName("MusicArtist");
                entity.Property(e => e.MusicDuration).HasColumnName("MusicDuration").HasDefaultValue(0);
                entity.Property(e => e.Duration).HasColumnName("Duration").HasDefaultValue(0);
                entity.Property(e => e.LikeCount).HasColumnName("LikeCount").HasDefaultValue(0);
                entity.Property(e => e.CommentCount).HasColumnName("CommentCount").HasDefaultValue(0);
                entity.Property(e => e.ShareCount).HasColumnName("ShareCount").HasDefaultValue(0);
                entity.Property(e => e.ViewCount).HasColumnName("ViewCount").HasDefaultValue(0);
                entity.Property(e => e.IsPublic).HasColumnName("IsPublic").HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.User).WithMany()
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_reels_user");
                entity.HasOne(d => d.Music).WithMany()
                    .HasForeignKey(d => d.MusicId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_reels_music");
            });

            // Reel Music
            modelBuilder.Entity<ReelMusic>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_reel_music");
                entity.ToTable("reel_music", "public");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Title).HasColumnName("Title").IsRequired().HasMaxLength(150);
                entity.Property(e => e.Artist).HasColumnName("Artist").HasMaxLength(100);
                entity.Property(e => e.MusicUrl).HasColumnName("MusicUrl").IsRequired();
                entity.Property(e => e.Duration).HasColumnName("Duration");
                entity.Property(e => e.CoverImageUrl).HasColumnName("CoverImageUrl");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Groups
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_groups");
                entity.ToTable("groups", "public");
                entity.HasIndex(e => e.Name, "ix_groups_name");
                entity.HasIndex(e => e.CreatedById, "ix_groups_created_by_id");
                entity.HasIndex(e => e.Privacy, "ix_groups_privacy");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("Name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasColumnName("Description").HasMaxLength(1000);
                entity.Property(e => e.AvatarUrl).HasColumnName("AvatarUrl").HasMaxLength(1024);
                entity.Property(e => e.CoverImageUrl).HasColumnName("CoverImageUrl").HasMaxLength(1024);
                entity.Property(e => e.CreatedById).HasColumnName("CreatedById");
                entity.Property(e => e.Privacy).HasColumnName("Privacy").IsRequired().HasMaxLength(20).HasDefaultValue("public");
                entity.Property(e => e.MemberCount).HasColumnName("MemberCount").HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.CreatedBy).WithMany(p => p.CreatedGroups)
                    .HasForeignKey(d => d.CreatedById)
                    .HasConstraintName("fk_groups_created_by_id");
            });

            // Group Members
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_group_members");
                entity.ToTable("group_members", "public");
                entity.HasIndex(e => e.GroupId, "ix_group_members_group_id");
                entity.HasIndex(e => e.UserId, "ix_group_members_user_id");
                entity.HasIndex(e => e.Status, "ix_group_members_status");
                entity.HasIndex(e => new { e.GroupId, e.UserId }, "uq_group_members_group_user").IsUnique();
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.GroupId).HasColumnName("GroupId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Role).HasColumnName("Role").IsRequired().HasMaxLength(20).HasDefaultValue("member");
                entity.Property(e => e.Status).HasColumnName("Status").IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.JoinedAt).HasColumnName("JoinedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.InvitedById).HasColumnName("InvitedById");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(d => d.Group).WithMany(p => p.Members)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("fk_group_members_group_id");
                entity.HasOne(d => d.User).WithMany(p => p.GroupMemberships)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_group_members_user_id");
                entity.HasOne(d => d.InvitedBy).WithMany()
                    .HasForeignKey(d => d.InvitedById)
                    .HasConstraintName("fk_group_members_invited_by_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
