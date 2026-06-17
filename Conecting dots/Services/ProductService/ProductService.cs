using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.AzureService;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.FileService;
using ConnectingDotsAPI.Services.HelperService;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using static ConnectingDotsAPI.Models.ProductModel;

namespace ConnectingDotsAPI.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly ConnectingDotsDbContext db;
        private readonly IHelperService helperService;
        private readonly IFileService fileService;
        private readonly ICacheService cacheService;
        public ProductService(ConnectingDotsDbContext db, IHelperService helperService, IFileService fileService, ICacheService cacheService)
        {
            this.db = db;
            this.helperService = helperService;
            ValidateProductType().Wait();
            this.fileService = fileService;
            this.cacheService = cacheService;
        }

        #region Products
        public async Task<List<ProductDetails>> GetProducts(string query)
        {
            var imageDownloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"product.image");
            return await db.Products.Where(x => !x.Deleted && (string.IsNullOrEmpty(query)?true:x.Name.ToLower().Contains(query.ToLower())))
                .Include(x => x.ProductType)
                            .Select(p => new ProductDetails
                            {
                                DisplayOrder = p.DisplayOrder,
                                Id = p.Guid,
                                Name = p.Name,
                                RedirectUrl = p.RedirectUrl,
                                Price = p.Price,
                                ProductCost = p.ProductCost,
                                ShortDescription = p.ShortDescription,
                                Sku = p.Sku,
                                ProductType = p.ProductType.Description,
                                Picture = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "product" && d.IsNew
                                && d.EntityId == p.Id && d.DownloadTypeId == imageDownloadTypeId).Select(d => new FileModel.FileDetails
                                {
                                    Id = d.Id,
                                    Blob = d.DownloadUrl,
                                    FileName = d.Filename,
                                    Url = d.DownloadUrl
                                }).FirstOrDefault(),
                                Published = p.Published,
                                Slug = db.UrlRecords
                                .Where(x =>
                                !string.IsNullOrEmpty(x.EntityName)
                                && x.EntityId == p.Id && x.EntityName == "Product" && x.IsActive).OrderByDescending(x => x.Id).First().Slug
                            }).ToListAsync();
        }
        public ProductDetails GetProductDetails(Guid? guid, int? id)
        {

            var imageDownloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"product.image");
            var attachmentDownloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"product.attachment");

            return db.Products.Where(x => x.Guid == guid || x.Id == id)
               
                            .Select(p => new ProductDetails
                            {
                                FullDescription = p.FullDescription,
                                DisplayOrder = p.DisplayOrder,
                                Id = p.Guid,
                                Categories = p.ProductCategoryMappings.Select(_x => new
                                {
                                    _x.Category.Name,
                                    _x.CategoryId,
                                    Slug = db.UrlRecords
                                .Where(x =>
                                !string.IsNullOrEmpty(x.EntityName)
                                && x.EntityId == _x.CategoryId && x.EntityName == "Category" && x.IsActive).OrderByDescending(x => x.Id).First().Slug
                                }),
                                ProductType = new { p.ProductType.Id, p.ProductType.SystemKeyword, p.ProductType.Description },
                                MetaDescription = p.MetaDescription,
                                MetaKeywords = p.MetaKeywords,
                                Name = p.Name,
                                RedirectUrl = p.RedirectUrl,
                                IsTaxExempt = p.IsTaxExempt,
                                TaxCategoryId = p.TaxCategoryId,
                                Price = p.Price,
                                ProductCost = p.ProductCost,
                                ShortDescription = p.ShortDescription,
                                Sku = p.Sku,
                                UserAgreementText = p.UserAgreementText,
                                Picture = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "product" && d.IsNew
                                && d.EntityId == p.Id && d.DownloadTypeId == imageDownloadTypeId).Select(d => new FileModel.FileDetails
                                {
                                    Id = d.Id,
                                    Blob = d.DownloadUrl,
                                    FileName = d.Filename,
                                    Url = d.DownloadUrl
                                }).FirstOrDefault(),
                                Attachments = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "product" && d.IsNew
                                && d.EntityId == p.Id && d.DownloadTypeId == attachmentDownloadTypeId).Select(d => new FileModel.FileDetails
                                {
                                    Id = d.Id,
                                    Blob = d.DownloadUrl,
                                    FileName = d.Filename,
                                    Url = d.DownloadUrl
                                }).ToList(),
                                Published = p.Published,
                                AdminComment = p.AdminComment,
                                HasUserAgreement = p.HasUserAgreement,
                                Forms = p.Forms.Where(x=>!x.Deleted).Select(x=> new { x.Description, x.Guid, x.Name}),
                                Form = p.Form != null ? new { p.Form.Name, p.Form.Guid, p.Form.Description } : null,
                                Slug = db.UrlRecords
                                .Where(x =>
                                !string.IsNullOrEmpty(x.EntityName)
                                && x.EntityId == p.Id && x.EntityName == "Product" && x.IsActive).OrderByDescending(x => x.Id).First().Slug

                            })
                            .First();
        }
        public async Task<Product> SaveProduct(ProductRequest request)
        {
            var val = new Product() { Guid = Guid.NewGuid(), Deleted = false };
            if (!string.IsNullOrEmpty(request.Guid))
                val = db.Products.FirstOrDefault(x => x.Guid == Guid.Parse(request.Guid));
            if (val == null) throw new Exception();
            val.ProductCost = request.ProductCost;
            val.Published = request.Published;
            val.ShortDescription = request.ShortDescription;
            val.Name = request.Name;
            val.RedirectUrl = request.RedirectUrl;
            val.AdminComment = request.AdminComment;
            val.MetaDescription = request.MetaDescription;
            val.FullDescription = request.FullDescription;
            val.DisplayOrder = request.DisplayOrder;
            val.HasUserAgreement = request.HasUserAgreement;
            val.UserAgreementText = request.UserAgreementText;
            val.IsTaxExempt = request.IsTaxExempt;
            val.TaxCategoryId = request.TaxCategoryId;
            val.Price = request.Price;
            val.Sku = request.Sku;
            val.UpdatedOnUtc = DateTime.UtcNow;
            val.MetaKeywords = request.MetaKeywords;
            val.ProductTypeId = request.ProductTypeId;
            if (!string.IsNullOrEmpty(request.FormId))
            {
                var form = db.Forms.Where(x => x.Guid == Guid.Parse(request.FormId)).FirstOrDefault();
                if (form != null)
                {
                    val.FormId = form.Id;
                }
            }

            if (val.Id == 0)
            {
                val.Deleted = false;
                val.CreatedOnUtc = DateTime.UtcNow;
                db.Products.Add(val);
            }
            await db.SaveChangesAsync();
            await helperService.UpdateUrlRecord(val.Id, "Product", request.Slug);

            return new Product { Guid = val.Guid, Id = val.Id };
        }

        public async Task<Product> DeleteProduct(string guid)
        {
            var val = db.Products.FirstOrDefault(p => p.Guid == Guid.Parse(guid)) ?? throw new Exception("NOT_FOUND");
            val.Deleted = true;
            await db.SaveChangesAsync();
            await helperService.DeleteUrlRecord(val.Id, "Product");
            return new Product { Id = val.Id, Guid = val.Guid };
        }
        public async Task<List<ProductType>> GetProductTypes()
        {
            return await db.ProductTypes.ToListAsync();
        }
        #endregion

        #region Product Category
        public async Task<ProductCategory> SaveCategory(ProductCategoryRequest request)
        {
            var val = new ProductCategory() { CreatedOnUtc = DateTime.Now, Guid = Guid.NewGuid(), Deleted = false };
            if (!string.IsNullOrEmpty(request.Guid))
                val = db.ProductCategories.FirstOrDefault(c => c.Guid == Guid.Parse(request.Guid));
            if (val == null) throw new Exception();
            val.UpdatedOnUtc = DateTime.UtcNow;
            val.Description = request.Description;
            val.MetaDescription = request.MetaDescription;
            val.MetaKeywords = request.MetaKeywords;
            val.DisplayOrder = request.DisplayOrder;
            val.IncludeInTopMenu = request.IncludeInTopMenu;
            val.MetaTitle = request.MetaTitle;
            val.Name = request.Name;
            val.ShowOnHomepage = request.ShowOnHomepage;
            val.IncludeInTopMenu = request.IncludeInTopMenu;
            val.ParentCategoryId = request.ParentCategoryId;
            val.PictureId = request.PictureId;
            val.Published = request.Published;
            val.DisplayOrder = request.DisplayOrder;
            val.FullDescription = request.FullDescription;
            if (val.Id == 0)
            {
                db.ProductCategories.Add(val);
            }
            await db.SaveChangesAsync();
            await helperService.UpdateUrlRecord(val.Id, "Category", request.Slug);
            //var key = $"Categories";
            //cacheService.Delete(key);
            return new ProductCategory { Guid = val.Guid, Id = val.Id, };
        }
        public async Task<(int categoryId, int productId)> MapProductCategory(MapProductCategoryRequest request)
        {
            var product = db.Products.Where(u => u.Guid == Guid.Parse(request.ProductGuid)).FirstOrDefault() ?? throw new Exception("PRODUCT NOT_FOUND");
            var category = db.ProductCategories.Where(u => u.Guid == Guid.Parse(request.CategoryGuid)).FirstOrDefault() ?? throw new Exception("CATEGORY NOT_FOUND");

            var mapping = db.ProductCategoryMappings.FirstOrDefault(x => x.ProductId == product.Id && x.CategoryId == category.Id);
            if (mapping != null)
            {

                if (request.Action == UserModel.MethodType.delete)
                {
                    db.ProductCategoryMappings.Remove(mapping);
                }
                else if (request.Action == UserModel.MethodType.edit)
                {
                    mapping.IsFeaturedProduct = request.IsFeaturedProduct ?? false;
                    mapping.DisplayOrder = request.DisplayOrder ?? 1;
                }
            }
            else
            {
                if (request.Action == UserModel.MethodType.insert)
                    db.ProductCategoryMappings.Add(new ProductCategoryMapping
                    {
                        ProductId = product.Id,
                        CategoryId = category.Id,
                        DisplayOrder = request.DisplayOrder ?? 1,
                        IsFeaturedProduct = request.IsFeaturedProduct ?? false,
                    });
            }
            await db.SaveChangesAsync();
            return (category.Id, product.Id);
        }
        public async Task<(int formId, int productId)> MapForm(MapProductFormRequest request)
        {
            var product = db.Products.Where(u => u.Guid == Guid.Parse(request.ProductId))
                .Include(x=>x.Forms)
                .FirstOrDefault() ?? throw new Exception("PRODUCT NOT_FOUND");
            var form = db.Forms.Where(u => u.Guid == Guid.Parse(request.FormId)).FirstOrDefault() ?? throw new Exception("FORM NOT_FOUND");

            if (request.Action == UserModel.MethodType.delete)
            {
                product.Forms.Remove(form);
            }
            else if (request.Action == UserModel.MethodType.insert)
                product.Forms.Add(form);


            await db.SaveChangesAsync();
            return (form.Id, product.Id);
        }



        public async Task<List<ProductCategoryDetails>> GetCategories()
        {
            //var key = $"Categories";
            //var cachedValue = cacheService.GetValue(key);
            //if (!string.IsNullOrEmpty(cachedValue))
            //{
                // return JsonConvert.DeserializeObject<List<ProductCategoryDetails>>(cachedValue);
            //}
            var categoryImageTypeId = helperService.FindByReferenceCodeAndType("download.type", "category.image");
            var categorydownloadIconTypeId = helperService.FindByReferenceCodeAndType("download.type", $"category.icon");
            var categorydownloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"category.image");
            var categoryIconTypeId = helperService.FindByReferenceCodeAndType("download.type", "category.icon");

            var response = await db.ProductCategories.Where(c => !c.Deleted)
                           .Select(c => new ProductCategoryDetails
                           {
                               Id = c.Id,
                               Description = c.Description,
                               DisplayOrder = c.DisplayOrder,
                               Guid = c.Guid,
                               IncludeInTopMenu = c.IncludeInTopMenu,
                               MetaDescription = c.MetaDescription,
                               MetaKeywords = c.MetaKeywords,
                               MetaTitle = c.MetaTitle,
                               Name = c.Name,
                               ParentCategoryId = c.ParentCategoryId,
                               Published = c.Published,
                               ShowOnHomepage = c.ShowOnHomepage,
                               Slug = db.UrlRecords.Where(x => x.EntityId == c.Id && x.EntityName == "Category" && x.IsActive).OrderByDescending(x => x.Id).First().Slug,
                               Picture = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "category" && d.IsNew
                                && d.EntityId == c.Id && d.DownloadTypeId == categorydownloadTypeId).Select(d => new FileModel.FileDetails
                                {
                                    Id = d.Id,
                                    Blob = d.DownloadUrl,
                                    FileName = d.Filename,
                                    Url = d.DownloadUrl
                                }).FirstOrDefault(),
                               Icon = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "category" && d.IsNew
                              && d.EntityId == c.Id && d.DownloadTypeId == categorydownloadIconTypeId).Select(d => new FileModel.FileDetails
                              {
                                  Id = d.Id,
                                  Blob = d.DownloadUrl,
                                  FileName = d.Filename,
                                  Url = d.DownloadUrl
                              }).FirstOrDefault(),
                           }).OrderBy(x => x.DisplayOrder)
                            .ToListAsync();

            //cacheService.SetValue(key, JsonConvert.SerializeObject(response));
            return response;
        }
        public ProductCategoryDetails? GetCategoryDetails(Guid? guid, int? id)
        {
            var categorydownloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"category.image");
            var categorydownloadIconTypeId = helperService.FindByReferenceCodeAndType("download.type", $"category.icon");
            var productdownloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"product.image");

            return db.ProductCategories.Where(x => x.Guid == guid || x.Id == id).Include(c => c.ProductCategoryMappings.Where(x=>!x.Product.Deleted))
                            .Select(c => new ProductCategoryDetails
                            {
                                Id = c.Id,
                                Description = c.Description,
                                DisplayOrder = c.DisplayOrder,
                                Guid = c.Guid,
                                IncludeInTopMenu = c.IncludeInTopMenu,
                                MetaDescription = c.MetaDescription,
                                MetaKeywords = c.MetaKeywords,
                                MetaTitle = c.MetaTitle,
                                Name = c.Name,
                                FullDescription = c.FullDescription,
                                ParentCategoryId = c.ParentCategoryId,
                                Picture = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "category" && d.IsNew
                                && d.EntityId == c.Id && d.DownloadTypeId == categorydownloadTypeId).Select(d => new FileModel.FileDetails
                                {
                                    Id = d.Id,
                                    Blob = d.DownloadUrl,
                                    FileName = d.Filename,
                                    Url = d.DownloadUrl
                                }).FirstOrDefault(),
                                Icon = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "category" && d.IsNew
                               && d.EntityId == c.Id && d.DownloadTypeId == categorydownloadIconTypeId).Select(d => new FileModel.FileDetails
                               {
                                   Id = d.Id,
                                   Blob = d.DownloadUrl,
                                   FileName = d.Filename,
                                   Url = d.DownloadUrl
                               }).FirstOrDefault(),
                                Published = c.Published,
                                ShowOnHomepage = c.ShowOnHomepage,
                                Products = c.ProductCategoryMappings.Where(x => !x.Product.Deleted)
                                .Select(x => new
                                {
                                    Product = x.Product.Name,
                                    ProductId = x.Product.Guid,
                                    x.IsFeaturedProduct,
                                    x.DisplayOrder,
                                    x.Product.ShortDescription,
                                    x.Product.RedirectUrl,
                                    ProductType = x.Product.ProductType.Description,
                                    x.Product.Price,
                                    Picture = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "product" && d.IsNew
                                && d.EntityId == x.ProductId && d.DownloadTypeId == productdownloadTypeId).Select(d => new FileModel.FileDetails
                                {
                                    Id = d.Id,
                                    Blob = d.DownloadUrl,
                                    FileName = d.Filename,
                                    Url = d.DownloadUrl
                                }).FirstOrDefault(),
                                    db.UrlRecords.Where(u => u.EntityId == x.ProductId && u.EntityName == "Product" && u.IsActive)
                                .OrderByDescending(u => u.Id).First().Slug
                                }).ToList(),
                                Slug = db.UrlRecords.Where(x => x.EntityId == c.Id && x.EntityName == "Category" && x.IsActive)
                                .OrderByDescending(x => x.Id).First().Slug,
                                Categories = db.ProductCategories.Where(_x => _x.ParentCategoryId == c.Id).Select(_x => new
                                {
                                    Slug = db.UrlRecords.Where(x => x.EntityId == _x.Id && x.EntityName == "Category" && x.IsActive)
                                .OrderByDescending(x => x.Id).First().Slug,
                                    Picture = db.Downloads.Where(d => !string.IsNullOrEmpty(d.EntityName) && d.EntityName.ToLower() == "category" && d.IsNew
                                    && d.EntityId == _x.Id && d.DownloadTypeId == categorydownloadTypeId).Select(d => new FileModel.FileDetails
                                    {
                                        Id = d.Id,
                                        Blob = d.DownloadUrl,
                                        FileName = d.Filename,
                                        Url = d.DownloadUrl
                                    }).FirstOrDefault(),
                                    _x.Name,
                                    _x.Description,

                                }).ToList()
                            })
                            .FirstOrDefault();
        }
        public async Task<ProductCategory> DeleteCategory(Guid guid)
        {
            var category = db.ProductCategories.FirstOrDefault(p => p.Guid == guid) ?? throw new Exception("NOT_FOUND");
            category.Deleted = true;
            await db.SaveChangesAsync();
            await helperService.DeleteUrlRecord(category.Id, "Category");
            //var key = $"Categories";
            //cacheService.Delete(key);
            return new ProductCategory { Id = category.Id, Guid = category.Guid };
        }
        #endregion

        #region Helpers
        //private async Task<Download> SaveDownload(string DownloadUrl, int entityId, string entityName, string fileName, int downloadTypeId)
        //{
        //    var download = new Download
        //    {
        //        DownloadTypeId = downloadTypeId,
        //        DownloadUrl = DownloadUrl,
        //        EntityId = entityId,
        //        EntityName = entityName,
        //        Filename = fileName,
        //    };
        //    db.Downloads.Add(download);
        //    await db.SaveChangesAsync();
        //    return new Download { Id = download.Id, DownloadGuid = download.DownloadGuid };
        //}

        public async Task<Download> SaveAttachment(Guid guid, string DownloadUrl, string fileName, string type, AttachmentType attachmentType)
        {
            var downloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"{type.ToLower()}.{attachmentType.ToString().ToLower()}") ?? throw new Exception("DOWNLOAD TYPE ID NOT FOUND");
            var result = await fileService.SaveDownload(
                new FileModel.DownloadUploadRequest
                {
                    BlobUrl = DownloadUrl,
                    EntityGuid = guid.ToString(),
                    EntityName = type,
                    Filename = fileName,
                    DownloadTypeId = downloadTypeId,
                });
            return result ?? throw new Exception("ERROR");
        }
        public async Task<Download> DeleteAttachment(int downloadId)
        {
            var download = db.Downloads.Where(d => d.Id == downloadId).FirstOrDefault() ?? throw new Exception("NOT_FOUND");
            db.Downloads.Remove(download);
            await db.SaveChangesAsync();
            return new Download { Id = download.Id, DownloadGuid = download.DownloadGuid };
        }
        public async Task<List<Download>> GetAttachments(int entityId, string type)
        {
            var downloadTypeId = helperService.FindByReferenceCodeAndType("download.type", $"{type}.attachment");
            if (downloadTypeId != null)
                return await db.Downloads.Where(d => d.DownloadTypeId == downloadTypeId
                && d.EntityName == type
                && d.EntityId == entityId).ToListAsync();
            return [];
        }
        #endregion

        private async Task ValidateProductType()
        {
            if (!db.ProductTypes.Any(x => x.SystemKeyword == "internal"))
            {
                db.ProductTypes.Add(new ProductType
                {
                    SystemKeyword = "internal",
                    Description = "Internal"
                });
            }
            if (!db.ProductTypes.Any(x => x.SystemKeyword == "redirect"))
            {
                db.ProductTypes.Add(new ProductType
                {
                    SystemKeyword = "redirect",
                    Description = "Redirect"
                });
            }

            await db.SaveChangesAsync();
        }

        private FileModel.FileDetails? GetDownloadFileDetails(int entityId, string entityName, int downloadTypeId)
        {
            return db.Downloads
                .Where(d => !string.IsNullOrEmpty(d.EntityName) && d.IsNew && d.EntityName.ToLower() == entityName && d.EntityId == entityId && d.DownloadTypeId == downloadTypeId)
                .Select(d => new FileModel.FileDetails
                {
                    Id = d.Id,
                    Blob = d.DownloadUrl,
                    FileName = d.Filename,
                    Url = d.DownloadUrl
                })
                .FirstOrDefault();
        }
    }
}
