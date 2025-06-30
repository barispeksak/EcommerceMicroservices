using Microsoft.EntityFrameworkCore;
using ProductCategoryMicroservice_Data.Entities;

namespace ProductCategoryMicroservice_Data
{
    public class CategoryDbContext : DbContext
    {
        public CategoryDbContext(DbContextOptions<CategoryDbContext> opts) : base(opts) { }

        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("category");
            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("product_category");

                e.HasKey(c => c.Id);

                // Otomatik artan ID için ekleme:
                e.Property(c => c.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                e.Property(c => c.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("category_name");

                // ParentCategoryId nullable olarak ayarla:
                e.Property(c => c.ParentCategoryId)
                    .HasColumnName("parent_category_id")
                    .IsRequired(false);

                // Self-reference ilişkisi:
                e.HasOne(c => c.ParentCategory)
                    .WithMany(c => c!.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ⭐️ UNİQUE INDEX BURAYA
                e.HasIndex(c => new { c.ParentCategoryId, c.CategoryName }).IsUnique();
            });
        }
    }
}
