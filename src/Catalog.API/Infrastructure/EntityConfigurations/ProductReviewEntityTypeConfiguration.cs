namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

class ProductReviewEntityTypeConfiguration
    : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("product_reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CatalogItemId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.ReviewText)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // One review per user per product
        builder.HasIndex(r => new { r.CatalogItemId, r.UserId })
            .IsUnique();

        // Efficient recent reviews query
        builder.HasIndex(r => new { r.CatalogItemId, r.CreatedAt })
            .IsDescending(false, true);

        // Foreign key relationship
        builder.HasOne<CatalogItem>()
            .WithMany()
            .HasForeignKey(r => r.CatalogItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
