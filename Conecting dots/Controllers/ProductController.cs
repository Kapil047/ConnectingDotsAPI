using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.FormService;
using ConnectingDotsAPI.Services.HelperService;
using ConnectingDotsAPI.Services.ProductService;
using ConnectingDotsAPI.Services.PropertyService;
using ConnectingDotsAPI.Services.SettingsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Newtonsoft.Json;
using static ConnectingDotsAPI.Models.ProductModel;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController(IProductService productService, IHelperService helperService, IAuthService authService, IActivityService activityService, IHttpContextAccessor httpContextAccessor, ICacheService cacheService) : Controller
    {
        readonly IProductService productService = productService;
        readonly IHelperService helperService = helperService;
        private readonly IAuthService authService = authService;
        private readonly IActivityService activityService = activityService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly ICacheService cacheService = cacheService;


        #region Product
        [HttpPost]
        [Route("records")]
        public async Task<IActionResult> GetAll([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);

                string cacheKey = $"product_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<ProductModel.ProductDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<ProductModel.ProductDetails>>(value);
                }
                else
                {
                    _result = await productService.GetProducts(string.Empty);
                    cacheService.SetValue(cacheKey, JsonConvert.SerializeObject(_result), 5);
                }

                var result = _result.AsQueryable();

                // Apply filtering
                param.Columns.ToList().ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.Search.Value))
                    {
                        var searchValue = p.Search.Value.ToLower();
                        switch (p.Data)
                        {
                            case "name":
                                result = result.Where(x => x.Name.ToLower().Contains(searchValue));
                                break;
                            case "sku":
                                result = result.Where(x => x.Sku != null && x.Sku.ToLower().Contains(searchValue));
                                break;
                            case "shortDescription":
                                result = result.Where(x => x.ShortDescription != null && x.ShortDescription.ToLower().Contains(searchValue));
                                break;
                            case "fullDescription":
                                result = result.Where(x => x.FullDescription != null && x.FullDescription.ToLower().Contains(searchValue));
                                break;
                            case "adminComment":
                                result = result.Where(x => x.AdminComment != null && x.AdminComment.ToLower().Contains(searchValue));
                                break;
                            case "metaDescription":
                                result = result.Where(x => x.MetaDescription != null && x.MetaDescription.ToLower().Contains(searchValue));
                                break;
                            case "metaKeywords":
                                result = result.Where(x => x.MetaKeywords != null && x.MetaKeywords.ToLower().Contains(searchValue));
                                break;
                            case "userAgreementText":
                                result = result.Where(x => x.UserAgreementText != null && x.UserAgreementText.ToLower().Contains(searchValue));
                                break;
                            case "price":
                                result = result.Where(x => x.Price.ToString().Contains(searchValue));
                                break;
                            case "productCost":
                                result = result.Where(x => x.ProductCost.ToString().Contains(searchValue));
                                break;
                            case "slug":
                                result = result.Where(x => x.Slug.ToLower().Contains(searchValue));
                                break;
                        }
                    }
                });

                // Apply sorting
                var sortOrder = param.Order.FirstOrDefault();
                if (sortOrder != null)
                {
                    var column = param.Columns[sortOrder.Column].Data;
                    var direction = sortOrder.Dir.ToLower();

                    switch (column)
                    {
                        case "name":
                            result = direction == "asc" ? result.OrderBy(x => x.Name) : result.OrderByDescending(x => x.Name);
                            break;
                        case "sku":
                            result = direction == "asc" ? result.OrderBy(x => x.Sku) : result.OrderByDescending(x => x.Sku);
                            break;
                        case "price":
                            result = direction == "asc" ? result.OrderBy(x => x.Price) : result.OrderByDescending(x => x.Price);
                            break;
                        case "productCost":
                            result = direction == "asc" ? result.OrderBy(x => x.ProductCost) : result.OrderByDescending(x => x.ProductCost);
                            break;
                        case "displayOrder":
                            result = direction == "asc" ? result.OrderBy(x => x.DisplayOrder) : result.OrderByDescending(x => x.DisplayOrder);
                            break;
                        case "published":
                            result = direction == "asc" ? result.OrderBy(x => x.Published) : result.OrderByDescending(x => x.Published);
                            break;
                        case "slug":
                            result = direction == "asc" ? result.OrderBy(x => x.Slug) : result.OrderByDescending(x => x.Slug);
                            break;
                    }
                }

                if (param.Length == -1) param.Length = _result.Count;
                return new JsonResult(new
                {
                    param.Draw,
                    Data = result.Skip((param.Start / param.Length) * param.Length).Take(param.Length).ToList(),
                    RecordsFiltered = result.Count(),
                    RecordsTotal = _result.Count
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }


        [Route("Types")]
        [OutputCache(Duration = 1 * 60 * 60)]
        [HttpGet]
        public async Task<IActionResult> GetProductTypes()
        {
            try
            {

                return Ok(await productService.GetProductTypes());
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }


       
        [OutputCache(Duration = 1 * 60 * 60)]
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var res = await productService.GetProducts(string.Empty);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [HttpPut]
        public async Task<IActionResult> SearchProducts([FromBody] string query)
        {
            try
            {
                var res = await productService.GetProducts(query);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductModel.ProductRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.SaveProduct(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = !string.IsNullOrEmpty(request.Guid) ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = !string.IsNullOrEmpty(request.Guid) ? $"Product UPDATE - {result}" : $"Form CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Product",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id, result.Guid } });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Authorize]
        [HttpDelete]
        [Route("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            try
            {


                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var response = await productService.DeleteProduct(guid);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Product DELETE - {guid}",
                    UserId = userId,
                    EntityId = response.Id,
                    EntityName = "Product",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });

            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [HttpPut]
        [Route("{guid}")]
        public IActionResult GetProductDetails(string guid)
        {
            try
            {

                if (!string.IsNullOrEmpty(guid))
                {
                    var res = productService.GetProductDetails(Guid.Parse(guid), null);
                    return Ok(res);


                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [OutputCache(Duration = 1 * 60 * 60)]
        [Route("details/{slug}")]
        [HttpGet]
        public IActionResult GetProductDetailsBySlug(string slug)
        {
            try
            {

                var id = helperService.GetEntityId("Product", slug);
                if (id != null)
                {
                    var res = productService.GetProductDetails(null, id.Value);
                    return Ok(res);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [HttpPost]
        [Route("category/records")]
        public async Task<IActionResult> GetCategories([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);

                string cacheKey = $"property_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<ProductCategoryDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<ProductCategoryDetails>>(value);
                }
                else
                {
                    _result = await productService.GetCategories();
                    cacheService.SetValue(cacheKey, JsonConvert.SerializeObject(_result), 5);
                }

                var result = _result.AsQueryable();

                param.Columns.ToList().ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.Search.Value))
                    {
                        var searchValue = p.Search.Value.ToLower();
                        switch (p.Data)
                        {
                            case "name":
                                result = result.Where(x => x.Name != null && x.Name.ToLower().Contains(searchValue));
                                break;
                            case "slug":
                                result = result.Where(x => x.Slug != null && x.Slug.ToLower().Contains(searchValue));
                                break;
                            case "metaKeywords":
                                result = result.Where(x => x.MetaKeywords != null && x.MetaKeywords.ToLower().Contains(searchValue));
                                break;
                            case "metaTitle":
                                result = result.Where(x => x.MetaTitle != null && x.MetaTitle.ToLower().Contains(searchValue));
                                break;
                            case "description":
                                result = result.Where(x => x.Description != null && x.Description.ToLower().Contains(searchValue));
                                break;
                            case "metaDescription":
                                result = result.Where(x => x.MetaDescription != null && x.MetaDescription.ToLower().Contains(searchValue));
                                break;
                            case "showOnHomepage":
                                result = result.Where(x => x.ShowOnHomepage.ToString().ToLower().Contains(searchValue));
                                break;
                            case "includeInTopMenu":
                                result = result.Where(x => x.IncludeInTopMenu.ToString().ToLower().Contains(searchValue));
                                break;
                            case "published":
                                result = result.Where(x => x.Published.ToString().ToLower().Contains(searchValue));
                                break;
                            case "displayOrder":
                                result = result.Where(x => x.DisplayOrder.ToString().ToLower().Contains(searchValue));
                                break;
                            case "fullDescription":
                                result = result.Where(x => x.FullDescription != null && x.FullDescription.ToLower().Contains(searchValue));
                                break;
                        }
                    }
                });

                // Apply sorting
                var sortOrder = param.Order.FirstOrDefault();
                if (sortOrder != null)
                {
                    var column = param.Columns[sortOrder.Column].Data;
                    var direction = sortOrder.Dir.ToLower();

                    switch (column)
                    {
                        case "name":
                            result = direction == "asc" ? result.OrderBy(x => x.Name) : result.OrderByDescending(x => x.Name);
                            break;
                        case "slug":
                            result = direction == "asc" ? result.OrderBy(x => x.Slug) : result.OrderByDescending(x => x.Slug);
                            break;
                        case "metaKeywords":
                            result = direction == "asc" ? result.OrderBy(x => x.MetaKeywords) : result.OrderByDescending(x => x.MetaKeywords);
                            break;
                        case "metaTitle":
                            result = direction == "asc" ? result.OrderBy(x => x.MetaTitle) : result.OrderByDescending(x => x.MetaTitle);
                            break;
                        case "description":
                            result = direction == "asc" ? result.OrderBy(x => x.Description) : result.OrderByDescending(x => x.Description);
                            break;
                        case "metaDescription":
                            result = direction == "asc" ? result.OrderBy(x => x.MetaDescription) : result.OrderByDescending(x => x.MetaDescription);
                            break;
                        case "parentCategoryId":
                            result = direction == "asc" ? result.OrderBy(x => x.ParentCategoryId) : result.OrderByDescending(x => x.ParentCategoryId);
                            break;
                        case "showOnHomepage":
                            result = direction == "asc" ? result.OrderBy(x => x.ShowOnHomepage) : result.OrderByDescending(x => x.ShowOnHomepage);
                            break;
                        case "includeInTopMenu":
                            result = direction == "asc" ? result.OrderBy(x => x.IncludeInTopMenu) : result.OrderByDescending(x => x.IncludeInTopMenu);
                            break;
                        case "published":
                            result = direction == "asc" ? result.OrderBy(x => x.Published) : result.OrderByDescending(x => x.Published);
                            break;
                        case "displayOrder":
                            result = direction == "asc" ? result.OrderBy(x => x.DisplayOrder) : result.OrderByDescending(x => x.DisplayOrder);
                            break;
                        case "fullDescription":
                            result = direction == "asc" ? result.OrderBy(x => x.FullDescription) : result.OrderByDescending(x => x.FullDescription);
                            break;
                    }
                }

                if (param.Length == -1) param.Length = _result.Count;
                return new JsonResult(new
                {
                    param.Draw,
                    Data = result.Skip((param.Start / param.Length) * param.Length).Take(param.Length).ToList(),
                    RecordsFiltered = result.Count(),
                    RecordsTotal = _result.Count
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }

        [Route("Category")]
        [OutputCache(Duration = 1 * 60 * 60)]
        [HttpGet]
        public async Task<IActionResult> GetProductCategories([FromQuery] string? f) //filter 
        {
            try
            {
                var res = await productService.GetCategories();
                if (!string.IsNullOrEmpty(f))
                {
                    if (f == "menu")
                        return Ok(res.Where(c => c.IncludeInTopMenu == true && c.Published && c.ParentCategoryId == 0).Select(c => new
                        {
                            c.Name,
                            c.Guid,
                            c.Slug,
                            c.Description,
                            c.Icon,
                            Submenu = res.Where(r => r.IncludeInTopMenu && r.ParentCategoryId == c.Id).Select(r => new
                            {
                                r.Id,
                                r.Name,
                                r.Slug,

                            }).ToList()
                        }).ToList());
                    else if (f == "homepage")
                        return Ok(res.Where(c => c.ShowOnHomepage && c.Published).Select(c => new
                        {
                            c.Name,
                            c.Guid,
                            c.Slug,
                            c.Icon
                        }).ToList());
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Route("CategoryDetails")]
        [OutputCache(Duration = 1 * 60 * 60)]
        [HttpGet]
        public IActionResult GetProductCategoryDetails([FromQuery] string? guid, string? sl)
        {
            try
            {
                int? id = null;
                Guid? _guid = null;
                if (!string.IsNullOrEmpty(sl))
                {
                    id = helperService.GetEntityId("Category", sl);

                }
                if (!string.IsNullOrEmpty(guid))
                {
                    _guid = Guid.Parse(guid);
                }
                var res = productService.GetCategoryDetails(_guid, id);
                return Ok(res);
                //return NotFound();

            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Route("Category/{guid}")]
        [HttpPut]
        public IActionResult GetProductCategoryDetails(string guid)
        {
            try
            {
                Guid? _guid = null;
                if (!string.IsNullOrEmpty(guid))
                {
                    _guid = Guid.Parse(guid);
                }
                var res = productService.GetCategoryDetails(_guid, null);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }

        [Authorize]
        [Route("Category")]
        [HttpPost]
        public async Task<IActionResult> SaveProductCategory(ProductModel.ProductCategoryRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.SaveCategory(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = !string.IsNullOrEmpty(request.Guid) ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = $"Category - {result.Guid}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Category",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id, result.Guid } });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Authorize]
        [Route("MapCategory")]
        [HttpPost]
        public async Task<IActionResult> MapProductCategory(ProductModel.MapProductCategoryRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.MapProductCategory(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Action == UserModel.MethodType.insert
                    ? activityService.GetActivityLogType("create")
                    : request.Action == UserModel.MethodType.delete
                    ? activityService.GetActivityLogType("delete")
                    : activityService.GetActivityLogType("update"),
                    Comment = $"Category Mapping - {result.categoryId} | {result.productId}",
                    UserId = userId,
                    EntityId = result.productId,
                    EntityName = "Product",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Authorize]
        [Route("MapForm")]
        [HttpPost]
        public async Task<IActionResult> MapProductForm(ProductModel.MapProductFormRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.MapForm(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Action == UserModel.MethodType.insert
                    ? activityService.GetActivityLogType("create")
                    : request.Action == UserModel.MethodType.delete
                    ? activityService.GetActivityLogType("delete")
                    : activityService.GetActivityLogType("update"),
                    Comment = $"Form Mapping - {result.formId} | {result.productId}",
                    UserId = userId,
                    EntityId = result.productId,
                    EntityName = "Product",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }

        [Authorize]
        [Route("Category/{guid}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProductCategory(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                    return BadRequest();

                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.DeleteCategory(Guid.Parse(guid));
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Category - {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Product",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        #endregion

        #region CommonOPS
        [Authorize]
        [Route("attachment")]
        [HttpPost]
        public async Task<IActionResult> SaveImage(ProductModel.ImageUploadRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.SaveAttachment(Guid.Parse(request.Guid), request.Url, request.FileName, request.Type, request.AttachmentType);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("create"),
                    Comment = $"Image - {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = request.Type,
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }

        [Route("Attachment")]
        [HttpGet]
        public async Task<IActionResult> GetAttachments(ProductModel.AttachmentsRequest request)
        {
            try
            {
                var res = await productService.GetAttachments(request.Id, request.Type);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        [Authorize]
        [Route("Attachment/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await productService.DeleteAttachment(id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Attachment- {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Product",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { ex.Message, ex.InnerException }));
            }
        }
        #endregion


        [OutputCache(Duration = 1 * 60 * 60)]
        [Route("TaxCategories"), HttpGet]
        public ActionResult GetTaxCategories()
        {
            try
            {
                return Ok((helperService.GetReferenceCodes("tax-category")).Where(x => x.Enabled).Select(x => new
                {
                    x.SystemKeyword,
                    x.Name,
                    x.Id
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

