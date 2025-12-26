using Amazon.Runtime.Internal;
using Azure.Core;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.HelperService;
using ConnectingDotsAPI.Services.SettingsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Newtonsoft.Json.Linq;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SettingsController(ISettingsService settingsService, IAuthService authService, IHttpContextAccessor httpContextAccessor, IActivityService activityService, IHelperService helperService) : ControllerBase
    {
        private readonly ISettingsService settingsService = settingsService;
        private readonly IAuthService authService = authService;
        private readonly IActivityService activityService = activityService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly IHelperService helperService = helperService;
        [Route("Country"), HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var result = await settingsService.GetCountries();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [Route("Country"), HttpPost]
        public async Task<IActionResult> SaveCountry([FromBody] SettingsModel.CountryRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await settingsService.SaveCountry(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Id.HasValue ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = request.Id.HasValue ? $"COUNTRY UPDATE - {result}" : $"COUNTRY CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Country",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("Country/{id}"), HttpGet]
        public IActionResult GetCountryDetails(int Id)
        {
            try
            {
                var res = settingsService.GetCountryDetails(Id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [Route("Country/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteCountry(int Id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await settingsService.DeleteCountry(Id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = 1,
                    Comment = $"COUNTRY DELETE - {Id}",
                    UserId = userId,
                    EntityId = Id,
                    EntityName = "Country",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("State"), HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveState([FromBody] SettingsModel.StateRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await settingsService.SaveState(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Id.HasValue ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = request.Id.HasValue ? $"STATE UPDATE - {result}" : $"STATE CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "State",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("State/{id}"), HttpGet]
        public IActionResult GetStateDetails(int id)
        {
            try
            {
                var res = settingsService.GetStateProvince(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("State/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteState(int id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await settingsService.DeleteState(id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"STATE DELETE - {id}",
                    UserId = userId,
                    EntityId = id,
                    EntityName = "State",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Route("City"), HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveCity([FromBody] SettingsModel.CityRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await settingsService.SaveCity(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Id.HasValue ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = request.Id.HasValue ? $"CITY UPDATE - {result}" : $"CITY CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "City",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("City/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteCity(int id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await settingsService.DeleteState(id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"City DELETE - {id}",
                    UserId = userId,
                    EntityId = id,
                    EntityName = "City",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("City/{id}"), HttpGet]
        public IActionResult GetCityDetails(int id)
        {
            try
            {
                var res = settingsService.GetCity(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [OutputCache(Duration = 1 * 60 * 60)]
        [Authorize]
        [Route("GetSitePages"), HttpGet]
        public async Task<IActionResult> GetSitePages()
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var _result = await settingsService.GetSitePages(userId);
                return Ok(_result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [OutputCache(Duration = 1 * 60 * 60)]
        [Route("GetPagesInRoles"), HttpPost]
        public async Task<IActionResult> GetPagesInRoles([FromBody] List<int> roleId)
        {
            try
            {
                var _result = await settingsService.GetPagesInRoles(roleId);
                return Ok(_result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("AddPageInRole"), HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPageInRole([FromBody] SettingsModel.PagesInRolesRequest values)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await settingsService.AddPageInRole(values);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("create"),
                    Comment = $"CREATE PAGE IN ROLE - {values.PagesId} | {values.RoleId}",
                    UserId = userId,
                    EntityId = values.RoleId,
                    EntityName = "Country",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("EmailTemplate"), HttpPost]
        public async Task<IActionResult> EmailTemplate(SettingsModel.EmailTemplateRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await settingsService.SaveEmailTemplate(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("create"),
                    Comment = $"CREATE EMAIL TEMPLATE - {result}",
                    UserId = userId,
                    EntityId = result,
                    EntityName = "Email Template",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("EmailTemplate"), HttpGet]
        public async Task<IActionResult> GetEmailTemplates()
        {
            try
            {
                var result = await settingsService.GetEmailTemplate();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("EmailTemplate/{id:int}"), HttpGet]
        public ActionResult EmailTemplateDetails(int id)
        {
            try
            {
                var result = settingsService.GetEmailTemplateDetails(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("DeleteEmailTemplate"), HttpPost]
        public async Task<ActionResult> DeleteEmailTemplate([FromBody] int id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                settingsService.DeleteEmailTemplate(id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"DELETE EMAIL TEMPLATE - {id}",
                    UserId = userId,
                    EntityId = id,
                    EntityName = "Email Template",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
