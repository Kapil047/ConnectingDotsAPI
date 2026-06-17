using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.CustomerService;
using ConnectingDotsAPI.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class CustomerController(ICustomerService CustomerService, IActivityService activityService, IAuthService authService, IHttpContextAccessor httpContextAccessor, ICacheService cacheService) : ControllerBase
    {
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly ICustomerService CustomerService = CustomerService;
        private readonly IActivityService activityService = activityService;
        private readonly IAuthService authService = authService;
        private readonly ICacheService cacheService = cacheService;
        
        [HttpPost]
        [Route("records")]
        [Authorize]
        public async Task<IActionResult> GetLeads([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);


                string cacheKey = $"customer_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<CustomerModel.CustomerDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<CustomerModel.CustomerDetails>>(value);
                }
                else
                {
                    _result = await CustomerService.GetAll();
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
                            case "email":
                                result = result.Where(x => x.Email != null && x.Email.ToLower().Contains(searchValue));
                                break;
                            // case "active":
                            //     result = result.Where(x => x.Active != null && x.PhoneNumber.ToLower().Contains(searchValue));
                            //     break;
                            // case "followUp":
                            //     result = result.Where(x => x.FollowUp != null && DateTime.Parse(x.FollowUp.ToString()).ToString("dd-MM-yyyy hh:mm").ToLower().Contains(searchValue));
                            //     break;
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
                        case "email":
                            result = direction == "asc" ? result.OrderBy(x => x.Email) : result.OrderByDescending(x => x.Email);
                            break;
                        case "active":
                            result = direction == "asc" ? result.OrderBy(x => x.Active) : result.OrderByDescending(x => x.Active);
                            break;
                        // case "followUp":
                        //     result = direction == "asc" ? result.OrderBy(x => x.FollowUp) : result.OrderByDescending(x => x.FollowUp);
                        //     break;
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
                return Ok((await CustomerService.GetAll()).Select(x => new
                {
                    x.Active,
                    x.AdminComment,
                    x.CreatedOnUtc,
                    x.Guid,
                    x.Email,
                    Id =x.Guid,
                    x.IsSystemAccount,
                    x.FirstName,
                    x.LastName,
                    x.Attributes,
                }));
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
                var id = await CustomerService.DeleteCustomer(Guid.Parse(guid));
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Customer DELETE - {guid}",
                    UserId = userId,
                    EntityId = id,
                    EntityName = "Customer",
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
        public IActionResult CustomerDetails(string guid)
        {

            try
            {
                var authorization = Request.Headers.Authorization.ToString();

                try
                {

                    return Ok(CustomerService.GetDetails(Guid.Parse(guid), null));
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

        [HttpGet]
        [Route("details")]
        public IActionResult CustomerDetailsLoggedIn()
        {

            try
            {
                var authorization = Request.Headers.Authorization.ToString();

                try
                {
                    int CustomerId = authService.GetCustomerId(authorization);
                    return Ok(CustomerService.GetDetails(null, CustomerId));
                }
                catch (Exception ex)
                {
                    return BadRequest($"INVALID_{ex}");

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Save(CustomerModel.CustomerRequest request)
        {
            try
            {
                var result = await CustomerService.SaveCustomer(request);
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
                    Comment = !string.IsNullOrEmpty(request.Guid) ? $"Customer UPDATE - {result}" : $"Customer CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Customer",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id, result.Guid } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("Address")]
        public async Task<IActionResult> SaveAddress(CustomerModel.AddressRequest request)
        {
            try
            {
                await CustomerService.SaveAddress(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("AddressGet")]
        public IActionResult GetAdddressDetails([FromBody] int addressId)
        {
            try
            {
                var res = CustomerService.GetAddressDetails(addressId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Route("MapAddress")]
        public async Task<IActionResult> MapCustomerAddress([FromBody] CustomerModel.MapAddressRequest request)
        {
            try
            {
                await CustomerService.MapCustomerAddress(request);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Route("DeleteCustomerAddress")]
        public async Task<IActionResult> DeleteCustomerAddress([FromBody] CustomerModel.DeleteAddressRequest request)
        {
            try
            {
                CustomerService.DeleteAddress(request);
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Customer ADDRESS DELETE - CustomerId:{request.CustomerId} AddressId:{request.AddressId}",
                    UserId = userId,
                    EntityId = request.AddressId,
                    EntityName = "CustomerAddress",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Route("Password")]
        public async Task<IActionResult> UpdatePassword([FromBody] AuthModel.UpdatePasswordRequest request)
        {
            try
            {
                await CustomerService.ChangeCustomerPassword(request);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }





    }
}
