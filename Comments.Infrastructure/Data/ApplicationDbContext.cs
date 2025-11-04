using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml.Linq;
using Comments.Application;
using Microsoft.EntityFrameworkCore;

namespace Comments.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Captcha> Captchas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.HomePage).HasMaxLength(500);
                entity.Property(u => u.UserIP).IsRequired().HasMaxLength(45);
                entity.Property(u => u.UserAgent).HasMaxLength(500);
                entity.Property(u => u.CreatedAt).IsRequired();

                entity.HasIndex(u => u.UserName);
                entity.HasIndex(u => u.Email);
                entity.HasIndex(u => u.CreatedAt);
            });

            // Comment configuration
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Text).IsRequired().HasMaxLength(5000);
                entity.Property(c => c.TextHtml).IsRequired();
                entity.Property(c => c.CreatedAt).IsRequired();
                entity.Property(c => c.FileName).HasMaxLength(255);
                entity.Property(c => c.FileExtension).HasMaxLength(10);
                entity.Property(c => c.FilePath).HasMaxLength(500);

                // Self-referencing relationship for replies
                entity.HasOne(c => c.Parent)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // User relationship
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.ParentId);
                entity.HasIndex(c => c.CreatedAt);
                entity.HasIndex(c => c.UserId);
            });

            // Captcha configuration
            modelBuilder.Entity<Captcha>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Code).IsRequired().HasMaxLength(10);
                entity.Property(c => c.ExpiresAt).IsRequired();
                entity.Property(c => c.CreatedAt).IsRequired();

                entity.HasIndex(c => c.ExpiresAt);
                entity.HasIndex(c => c.IsUsed);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    if (entity is User user)
                    {
                        user.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entity is Comment comment)
                    {
                        comment.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entity is Captcha captcha)
                    {
                        captcha.CreatedAt = DateTime.UtcNow;
                    }
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    if (entity is Comment comment)
                    {
                        comment.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entity is User user)
                    {
                        user.LastActivity = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
