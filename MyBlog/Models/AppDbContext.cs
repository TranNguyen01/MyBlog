using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyBlog.Models
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex("Slug").IsUnique();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex("Slug").IsUnique();

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Posts)
                    .HasForeignKey(p => p.CategoryId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Author)
                   .WithMany(a => a.Posts)
                   .HasForeignKey(p => p.AuthorId)
                   .IsRequired(false);
                //.OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Thumbnail)
                    .WithOne(p => p.Post)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(c => c.CreatedAt).HasColumnType("datetime2");
                entity.HasOne(c => c.User).WithMany(u => u.Comments).OnDelete(DeleteBehavior.NoAction);

            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasOne(l => l.Post).WithMany(p => p.Likes).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(l => l.User).WithMany(p => p.Likes).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(p => p.Avatar)
                 .WithOne(p => p.User)
                 .IsRequired(false);
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasOne(p => p.Post)
                    .WithOne(p => p.Thumbnail)
                    .HasForeignKey<Post>(p => p.ThumbnailId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.Avatar)
                    .HasForeignKey<User>(u => u.AvatarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<Photo> Photos { get; set; }
    }
}
