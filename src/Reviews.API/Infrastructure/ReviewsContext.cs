using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Reviews.API.Infrastructure;

public class ReviewsContext : DbContext
{
    public ReviewsContext(DbContextOptions<ReviewsContext> options) : base(options)
    {
    }

    public required DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ReviewEntityTypeConfiguration());
    }
}

internal class ReviewEntityTypeConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.ReviewText)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.UserId);
    }
}
