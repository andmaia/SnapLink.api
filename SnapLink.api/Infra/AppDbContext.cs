using Microsoft.EntityFrameworkCore;
using SnapLink.Api.Domain;
using SnapLink.api.Domain;

namespace SnapLink.api.Infra
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Page> Pages { get; set; }
        public DbSet<PageFile> PageFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Page>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).IsRequired();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);

                entity.HasMany(p => p.PageFiles)
                      .WithOne(f => f.Page)
                      .HasForeignKey(f => f.PageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PageFile>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).IsRequired();
                entity.Property(f => f.FileName).HasMaxLength(255).IsRequired(false);
                entity.Property(f => f.ContentType).IsRequired(false);
                entity.Property(f => f.Data).IsRequired(false); // bytea automaticamente
                entity.Property(f => f.ExpiresAt).IsRequired();
                entity.Property(f => f.FinishedAT).IsRequired(false);
            });
        }



    }
}
