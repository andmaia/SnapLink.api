namespace SnapLink.api.Infra
{
    using global::SnapLink.Api.Domain;
    using Microsoft.EntityFrameworkCore;

    namespace SnapLink.Api.Infra
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
            {
            }

            public DbSet<Page> Pages { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Page>(entity =>
                {
                    entity.HasKey(p => p.Id); 
                    entity.Property(p => p.Name)
                          .IsRequired()
                          .HasMaxLength(100);
                });
            }
        }
    }

}
