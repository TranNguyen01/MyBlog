using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

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
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(c => c.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValue(DateTime.Now);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => new { l.UserId, l.PostId });
                entity.HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.User)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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
                    .HasForeignKey<Photo>(p => p.PostId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.Avatar)
                    .HasForeignKey<Photo>(p => p.UserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Collections>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Collections)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne(c => c.ParentCollections)
                    .WithMany(c => c.ChildrenCollections)
                    .HasForeignKey(c => c.ParentCollectionsId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PostCollection>(entity =>
            {
                entity.HasKey(pc => new { pc.PostId, pc.CollectionsId });

                entity.HasOne(pc => pc.Collections)
                    .WithMany(c => c.PostCollections)
                    .HasForeignKey(pc => pc.CollectionsId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Post)
                    .WithMany()
                    .HasForeignKey(pc => pc.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                //entity.HasIndex("Slug").IsUnique();

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Documents)
                    .HasForeignKey(p => p.CategoryId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Author)
                   .WithMany(a => a.Documents)
                   .HasForeignKey(p => p.AuthorID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            });

            modelBuilder.Entity<DocumentCollection>(entity =>
            {
                //entity.HasIndex("Slug").IsUnique();
                entity.HasKey(dc => new { dc.DocumentId, dc.CollectionsId });

                entity.HasOne(p => p.Collection)
                    .WithMany(c => c.DocumentCollections)
                    .HasForeignKey(p => p.CollectionsId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Document)
                   .WithMany()
                   .HasForeignKey(p => p.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(p => p.Post)
                    .WithMany()
                    .HasForeignKey(p => p.PostId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Document)
                   .WithMany()
                   .HasForeignKey(p => p.DocumentId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Reason)
                  .WithMany()
                  .HasForeignKey(p => p.ReasonId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Censorship>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(p => p.Reason)
                    .WithMany()
                    .HasForeignKey(p => p.ReasonId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Post)
                    .WithMany()
                    .HasForeignKey(p => p.PostId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Document)
                    .WithMany()
                    .HasForeignKey(p => p.DocumentId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Notify>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CensorCategory>(entity =>
            {
                entity.HasKey(c => new { c.UserId, c.CategoryId });

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .IsRequired(true)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Category)
                    .WithMany()
                    .HasForeignKey(c => c.CategoryId)
                    .IsRequired(true)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<Collections> Collections { get; set; }

        public DbSet<PostCollection> PostCollections { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentCollection> DocumentCollections { get; set; }
        public DbSet<Notify> Notify { get; set; }
        public DbSet<Censorship> Censorships { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Reason> Reasons { get; set; }
        public DbSet<CensorCategory> CensorCategories { get; set; }
    }
}
