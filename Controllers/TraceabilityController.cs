using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.HelperService;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.traceabilityService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.TopicsService;
using Microsoft.AspNetCore.Authorization;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TraceabilityController(ITraceabilityService traceabilityService, IHelperService helperService, IActivityService activityService, IAuthService authService, IHttpContextAccessor httpContextAccessor, ICacheService cacheService) : ControllerBase
    {
        private readonly ITraceabilityService traceabilityService = traceabilityService;
        private readonly IHelperService helperService = helperService;
        private readonly IActivityService activityService = activityService;
        private readonly IAuthService authService = authService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly ICacheService cacheService = cacheService;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Save(TraceabilityModel.TraceabilityRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await traceabilityService.Save(request, userId);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Id.HasValue ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = request.Id.HasValue ? $"UPDATE - {result}" : $"CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Topic",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} INNER {ex.InnerException}");
            }
        }
        [Route("{id}"), HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await traceabilityService.Delete(new TraceabilityModel.TraceabilityRequest { Guid =  id});
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"DELETE - {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Topic",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("{id}"), HttpGet]
        public IActionResult Details(string id)
        {
            try
            {
                var result = traceabilityService.Details(new TraceabilityModel.TraceabilityRequest { Guid = id});
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("types"), HttpGet]
        public IActionResult GetTypes()
        {
            try
            {
                var result = helperService.GetReferenceCodes("fabric.type");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
