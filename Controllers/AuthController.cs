using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Models.Auth;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.CustomerService;
using ConnectingDotsAPI.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService, IUserService userService, IActivityService activityService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IAuthService authService = _authService;
        private readonly IUserService userService = userService;
        private readonly IActivityService activityService = activityService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await authService.Login(model);
            if (result.Result)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthModel.RegisterRequest model)
        {
            try
            {
                await authService.Register(model);
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }
        [Authorize]
        [HttpPost("refreshToken")]
        public IActionResult RefreshToken(AuthModel.IValidTokenRequest request)
        {
            try
            {
                //if (request.Type == AuthModel.LoginType.Admin)
                return Ok(authService.IsTokenValid(request.Token));
                //if (request.Type == AuthModel.LoginType.Customer)
                //    return Ok(authService.IsCustomerTokenValid(request.Token));
                //return BadRequest("INVALID");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        #region Roles
        [HttpGet]
        [Authorize]
        [Route("Role")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = (await userService.GetRoles(userId)).Select(u => new { u.Name, u.SystemName, u.Id, u.Pages });
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        [Route("RoleDetails")]
        public IActionResult GetRoleDetails([FromBody] int roleId)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = userService.GetRoleDetails(roleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        [Route("UserRole")]
        public async Task<IActionResult> SaveUserRole([FromBody] UserModel.SaveRoleRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await userService.SaveRole(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"USER ROLE - {request.Id}",
                    UserId = userId,
                    EntityId = request.Id,
                    EntityName = "UserRole",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        [Route("Role")]
        public async Task<IActionResult> AddRole([FromBody] UserModel.UpdateRoleRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                userService.UpdateRole(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"USER ROLE UPDATE - {request.RoleId} | {request.UserId}",
                    UserId = userId,
                    EntityId = request.RoleId,
                    EntityName = "Role",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });

            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("AssignRole")]
        public async Task<IActionResult> AssignParentRole([FromBody] UserModel.UpdateRoleAssigment request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await userService.UpdateRoleAssignment(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType(request.Action.ToString()),
                    Comment = $"USER ROLE ASSIGNMENT - {request.RoleId} | {request.ParentRoleId}",
                    UserId = userId,
                    EntityId = request.RoleId,
                    EntityName = "Role",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });

            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
