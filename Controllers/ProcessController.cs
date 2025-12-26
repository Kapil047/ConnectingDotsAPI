using Azure.Core;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.HelperService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProcessController(IHelperService helperService, IActivityService activityService, IAuthService authService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IHelperService helperService = helperService;
        private readonly IActivityService activityService = activityService;
        private readonly IAuthService authService = authService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

        [HttpGet]
        public IActionResult GetProcesses()
        {
            try
            {
                return Ok(helperService.GetReferenceCodes("process").Select(x=> new
                {
                    x.Id, x.SystemKeyword, x.Name
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("{code}")]
        public IActionResult GetProcessDetails(string code)
        {
            try
            {
                return Ok(helperService.GetReferenceCodeDetails("process", code));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveProcess([FromBody] string name)
        {
            try
            {
                var referenceTypeId = helperService.FindByReferenceType("process");
                if (!referenceTypeId.HasValue)
                    return BadRequest("REFERENCE TYPE PROCESS NOT FOUND");

                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await helperService.SaveReferenceCode(referenceTypeId.Value, name.Trim().Replace(' ', '.').ToLower(), name);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("create"),
                    Comment = $"PROCESS CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Reference Code",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.SystemKeyword, result.Name, result.Id } });

            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} | {ex.InnerException}");
            }
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] string code)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await helperService.DeleteReferenceCode(code.Trim());
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"PROCESS DELETE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Reference Code",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.SystemKeyword, result.Name, result.Id } });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
