using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.WebAppComponents.Catalog;

namespace eShop.WebAppComponents.Services
{
    public interface ICatalogService
    {
        Task<CatalogItem?> GetCatalogItem(int id);
        Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type);
        Task<List<CatalogItem>> GetCatalogItems(IEnumerable<int> ids);
        Task<CatalogResult> GetCatalogItemsWithSemanticRelevance(int page, int take, string text);
        Task<IEnumerable<CatalogBrand>> GetBrands();
        Task<IEnumerable<CatalogItemType>> GetTypes();
        Task<PaginatedReviewsDto> GetReviews(int itemId, int pageIndex = 0, int pageSize = 10);
        Task<ReviewDto?> GetUserReview(int itemId);
        Task<ReviewDto?> SubmitReview(int itemId, int rating, string? reviewText);
    }
}
