using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.CustomerService;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConnectingDotsAPI.Services.CacheService;
using Newtonsoft.Json;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService, IHttpContextAccessor httpContextAccessor, IAuthService authService, IActivityService activityService, ICacheService cacheService) : ControllerBase
    {
        private readonly IUserService userService = userService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly IAuthService authService = authService;
        private readonly IActivityService activityService = activityService;
        private readonly ICacheService cacheService = cacheService;
        [HttpPost]
        [Route("records")]
        [Authorize]
        public async Task<IActionResult> GetAll([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);


                string cacheKey = $"user_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<UserModel.UserDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<UserModel.UserDetails>>(value);
                }
                else
                {
                    _result = await userService.GetAll(userId,null);
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
                            case "firstName":
                                result = result.Where(x => x.FirstName != null && x.FirstName.ToLower().Contains(searchValue));
                                break;
                            case "lastName":
                                result = result.Where(x => x.LastName != null && x.LastName.ToString().ToLower().Contains(searchValue));
                                break;
                            case "username":
                                result = result.Where(x => x.Username != null && x.Username.ToLower().Contains(searchValue));
                                break;
                            case "parentUser":
                                result = result.Where(x => x.ParentUser != null && x.ParentUser.ToLower().Contains(searchValue));
                                break;
                            case "roles":
                                result = result.Where(x => x.Roles != null && x.Roles.ToString().ToLower().Contains(searchValue));
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
                        case "firstName":
                            result = direction == "asc" ? result.OrderBy(x => x.FirstName) : result.OrderByDescending(x => x.FirstName);
                            break;
                        case "lastName":
                            result = direction == "asc" ? result.OrderBy(x => x.LastName) : result.OrderByDescending(x => x.LastName);
                            break;
                        case "username":
                            result = direction == "asc" ? result.OrderBy(x => x.Username) : result.OrderByDescending(x => x.Username);
                            break;
                        case "parentUser":
                            result = direction == "asc" ? result.OrderBy(x => x.ParentUser) : result.OrderByDescending(x => x.ParentUser);
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


        [HttpPost]
        public async Task<IActionResult> Save(UserModel.IUserSaveRequest request)
        {
            try
            {
                var result = await userService.Save(request);
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = !string.IsNullOrEmpty(request.Guid) ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = !string.IsNullOrEmpty(request.Guid) ? $"USER UPDATE - {result.Id}" : $"USER CREATE - {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "User",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} {ex.InnerException}");
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] string? f)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                return Ok((await userService.GetAll(userId, f)).Select(x => new
                {
                    x.Active,
                    x.Guid,
                    x.FirstName,
                    x.LastName,
                    x.ParentUser,
                     x.Roles,
                    x.Username
                }));
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
        public async Task<ActionResult> GetDetails(string guid)
        {
            try
            {
                return Ok(await userService.GetDetails(Guid.Parse(guid), null));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet, Route("details")]
        public async Task<ActionResult> GetUserDetails()
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                return Ok(await userService.GetDetails(null, userId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            try
            {
                var id = await userService.Delete(Guid.Parse(guid));
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"USER DELETE - {guid}",
                    UserId = userId,
                    EntityId = id,
                    EntityName = "User",
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

        [HttpPost]
        [Route("Password")]
        public async Task<IActionResult> UpdatePassword([FromBody] AuthModel.UpdatePasswordRequest request)
        {
            try
            {
                await userService.ChangePassword(request);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
