using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using static ConnectingDotsAPI.Models.ProductModel;

namespace ConnectingDotsAPI.Services.ProductService
{
    public interface IProductService
    {
        Task<Download> DeleteAttachment(int downloadId);
        Task<ProductCategory> DeleteCategory(Guid guid);
        Task<Product> DeleteProduct(string guid);
        Task<List<Download>> GetAttachments(int entityId, string type);
        Task<List<ProductModel.ProductCategoryDetails>> GetCategories();
        ProductModel.ProductCategoryDetails? GetCategoryDetails(Guid? guid, int? id);
        ProductModel.ProductDetails GetProductDetails(Guid? guid, int? id);
        Task<List<ProductType>> GetProductTypes();
         Task<(int categoryId, int productId)> MapProductCategory(ProductModel.MapProductCategoryRequest request);
        Task<Download> SaveAttachment(Guid productGuid, string DownloadUrl, string fileName, string type, ProductModel.AttachmentType attachmentType);
        Task<ProductCategory> SaveCategory(ProductModel.ProductCategoryRequest request);
        //Task<Download> SaveIcon(int id, string DownloadUrl, string fileName, string type);
        //Task<Download> SaveImage(int id, string DownloadUrl, string fileName, string type);
        Task<Product> SaveProduct(ProductModel.ProductRequest request);
        Task<(int formId, int productId)> MapForm(MapProductFormRequest request);
        Task<List<ProductDetails>> GetProducts(string query);
    }
}