using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.PropertyService;
using ConnectingDotsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.Runtime.Internal;
using ConnectingDotsAPI.Services.CacheService;
using Newtonsoft.Json;

namespace ConnectingDotsAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class PropertyController(IPropertyService propertyService, IHttpContextAccessor httpContextAccessor, IActivityService activityService, IAuthService authService, ICacheService cacheService) : ControllerBase
    {
        private readonly IPropertyService propertyService = propertyService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly IActivityService activityService = activityService;
        private readonly IAuthService authService = authService;
        private readonly ICacheService cacheService = cacheService;

        [HttpPost]
        [Route("records")]
        public async Task<IActionResult> GetAll([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);


                string cacheKey = $"property_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<PropertyModel.PropertyDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<PropertyModel.PropertyDetails>>(value);
                }
                else
                {
                    _result = await propertyService.GetAll();
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
                            case "city":
                                result = result.Where(x => x.City != null && x.City.ToLower().Contains(searchValue));
                                break;
                            case "propertyName":
                                result = result.Where(x => x.PropertyName != null && x.PropertyName.ToString().ToLower().Contains(searchValue));
                                break;
                            case "propertyType":
                                result = result.Where(x => x.PropertyType != null && x.PropertyType.ToLower().Contains(searchValue));
                                break;
                            case "addressLine1":
                                result = result.Where(x => x.AddressLine1 != null && x.AddressLine1.ToLower().Contains(searchValue));
                                break;
                            case "addressLine2":
                                result = result.Where(x => x.AddressLine2 != null && x.AddressLine2.ToLower().Contains(searchValue));
                                break;
                            case "state":
                                result = result.Where(x => x.State != null && x.State.ToString().ToLower().Contains(searchValue));
                                break;
                            case "country":
                                result = result.Where(x => x.Country != null && x.Country.ToString().ToLower().Contains(searchValue));
                                break;
                            case "bathrooms":
                                result = result.Where(x => x.Bathrooms.ToString().Contains(searchValue));
                                break;
                            case "bedrooms":
                                result = result.Where(x => x.Bedrooms.ToString().Contains(searchValue));
                                break;
                            case "listingDate":
                                result = result.Where(x => x.ListingDate.ToString("dd-MM-yyyy hh:mm").ToLower().Contains(searchValue));
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
                        case "propertyName":
                            result = direction == "asc" ? result.OrderBy(x => x.PropertyName) : result.OrderByDescending(x => x.PropertyName);
                            break;
                        case "city":
                            result = direction == "asc" ? result.OrderBy(x => x.City) : result.OrderByDescending(x => x.City);
                            break;
                        case "propertyType":
                            result = direction == "asc" ? result.OrderBy(x => x.PropertyType) : result.OrderByDescending(x => x.PropertyType);
                            break;
                        case "addressLine1":
                            result = direction == "asc" ? result.OrderBy(x => x.AddressLine1) : result.OrderByDescending(x => x.AddressLine1);
                            break;
                        case "addressLine2":
                            result = direction == "asc" ? result.OrderBy(x => x.AddressLine2) : result.OrderByDescending(x => x.AddressLine2);
                            break;
                        case "state":
                            result = direction == "asc" ? result.OrderBy(x => x.State) : result.OrderByDescending(x => x.State);
                            break;
                        case "country":
                            result = direction == "asc" ? result.OrderBy(x => x.Country) : result.OrderByDescending(x => x.Country);
                            break;
                        case "bathrooms":
                            result = direction == "asc" ? result.OrderBy(x => x.Bathrooms) : result.OrderByDescending(x => x.Bathrooms);
                            break;
                        case "bedrooms":
                            result = direction == "asc" ? result.OrderBy(x => x.Bedrooms) : result.OrderByDescending(x => x.Bedrooms);
                            break;
                        case "listingDate":
                            result = direction == "asc" ? result.OrderBy(x => x.ListingDate) : result.OrderByDescending(x => x.ListingDate);
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



        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                return Ok(await propertyService.GetAll());
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            try
            {
                var response = await propertyService.Delete(Guid.Parse(guid));
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Property DELETE - {guid}",
                    UserId = userId,
                    EntityId = response.Id,
                    EntityName = "Property",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Route("{guid}")]
        public IActionResult Details(string guid)
        {

            try
            {
                var authorization = Request.Headers.Authorization.ToString();

                try
                {

                    return Ok(propertyService.GetDetails(Guid.Parse(guid), null));
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);


                }
            }
            catch
            {
                return BadRequest("INVALID");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Save(PropertyModel.PropertyRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Title = "One or more validation errors occurred.",
                    Status = 400,
                    Errors = new Dictionary<string, string[]>()
                };

                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    var errorMessages = new List<string>();
                    foreach (var error in errors)
                    {
                        errorMessages.Add(error.ErrorMessage);
                    }
                    errorResponse.Errors.Add(key, errorMessages.ToArray());
                }

                return BadRequest(errorResponse);
            }
            try
            {
                var result = await propertyService.Save(request);
                var authorization = Request.Headers.Authorization.ToString();
                int? userId;
                try
                {
                    userId = authService.GetUserId(authorization);
                }
                catch
                {
                    userId = null;
                }

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = !string.IsNullOrEmpty(request.Guid) ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = !string.IsNullOrEmpty(request.Guid) ? $"Property UPDATE - {result}" : $"Property CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Property",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id, result.Guid } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    public class ErrorResponse
    {
        public string Title { get; set; }
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
