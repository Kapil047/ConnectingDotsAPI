using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.FormService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using ConnectingDotsAPI.Services.CacheService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using static ConnectingDotsAPI.Models.FormModel;


namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FormController(IFormService formService, IHttpContextAccessor httpContextAccessor, IActivityService activityService, IAuthService authService, ICacheService cacheService) : ControllerBase
    {
        private readonly IFormService formService = formService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly IActivityService activityService = activityService;
        private readonly IAuthService authService = authService;
        private readonly ICacheService cacheService = cacheService;
        #region  Form

        [HttpPost]
        [Route("records")]
        [Authorize]
        public async Task<IActionResult> GetLeads([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);


                string cacheKey = $"leads_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<FormModel.FormDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<FormModel.FormDetails>>(value);
                }
                else
                {
                    _result = await formService.GetAll();
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
                            case "description":
                                result = result.Where(x => x.Description != null && x.Description.ToString().ToLower().Contains(searchValue));
                                break;
                            case "displayOrder":
                                result = result.Where(x => x.DisplayOrder.ToString().Contains(searchValue));
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
                        case "name":
                            result = direction == "asc" ? result.OrderBy(x => x.Name) : result.OrderByDescending(x => x.Name);
                            break;
                        case "description":
                            result = direction == "asc" ? result.OrderBy(x => x.Description) : result.OrderByDescending(x => x.Description);
                            break;
                        case "displayOrder":
                            result = direction == "asc" ? result.OrderBy(x => x.DisplayOrder) : result.OrderByDescending(x => x.DisplayOrder);
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


        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                return Ok((await formService.GetAll()).Select(form => new
                {
                    form.Guid,
                    form.Name,
                    form.Active
                }));
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpDelete]
        [Route("{guid}")]
        public async Task<IActionResult> Delete(string guid)
        {
            try
            {
                var response = await formService.Delete(Guid.Parse(guid));
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Form DELETE - {guid}",
                    UserId = userId,
                    EntityId = response.Id,
                    EntityName = "Form",
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

                    return Ok(formService.GetDetails(Guid.Parse(guid), null));
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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save(FormModel.FormRequest request)
        {
            try
            {
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
                var result = await formService.SaveForm(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = !string.IsNullOrEmpty(request.Guid) ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = !string.IsNullOrEmpty(request.Guid) ? $"Form UPDATE - {result}" : $"Form CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Form",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id, result.Guid } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost]
        [Route("submit")]
        public async Task<IActionResult> SubmitResponse(FormModel.FormResponseRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                int? customerId;
                try
                {
                    customerId = authService.GetCustomerId(authorization);
                }
                catch
                {
                    customerId = null;
                }
                if (customerId == null) throw new Exception("INVALID CUSTOMERʼ");
                await formService.SubmitAnswer(request, customerId.Value);


                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} | INNER : {ex.InnerException}");
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("response/{id}")]
        public async Task<IActionResult> DeleteResponse(int id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var response = await formService.DeleteResponse(id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Response DELETE - {id}",
                    UserId = userId,
                    EntityId = response.Id,
                    EntityName = "Response",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true });

            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message} | INNER : {ex.InnerException}");
            }
        }

        #endregion Form

        #region Question


        [Authorize]
        [HttpGet]
        [Route("question")]
        public async Task<ActionResult> GetAllQuestions()
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                return Ok((await formService.GetAllQuestions()).Select(form => new
                {
                    form.Guid,
                    form.Text,
                    form.ControlType,
                    form.Active
                }));
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("question/records")]
        public async Task<IActionResult> GetAllQuestions([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);

                string cacheKey = $"question_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<FormModel.QuestionDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<FormModel.QuestionDetails>>(value);
                }
                else
                {
                    _result = await formService.GetAllQuestions();
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
                            case "text":
                                result = result.Where(x => x.Text.ToLower().Contains(searchValue));
                                break;
                            case "controlType":
                                result = result.Where(x => x.ControlType.ToLower().Contains(searchValue));
                                break;
                            case "active":
                                result = result.Where(x => x.Active.ToString().ToLower().Contains(searchValue));
                                break;
                            case "displayOrder":
                                result = result.Where(x => x.DisplayOrder.ToString().ToLower().Contains(searchValue));
                                break;
                            case "options":
                                result = result.Where(x => x.Options != null && x.Options.ToString().ToLower().Contains(searchValue));
                                break;
                            case "questionResponses":
                                result = result.Where(x => x.QuestionResponses != null && x.QuestionResponses.ToString().ToLower().Contains(searchValue));
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
                        case "text":
                            result = direction == "asc" ? result.OrderBy(x => x.Text) : result.OrderByDescending(x => x.Text);
                            break;
                        case "controlType":
                            result = direction == "asc" ? result.OrderBy(x => x.ControlType) : result.OrderByDescending(x => x.ControlType);
                            break;
                        case "active":
                            result = direction == "asc" ? result.OrderBy(x => x.Active) : result.OrderByDescending(x => x.Active);
                            break;
                        case "displayOrder":
                            result = direction == "asc" ? result.OrderBy(x => x.DisplayOrder) : result.OrderByDescending(x => x.Active);
                            break;
                        case "options":
                            result = direction == "asc" ? result.OrderBy(x => x.Options) : result.OrderByDescending(x => x.Options);
                            break;
                        case "questionResponses":
                            result = direction == "asc" ? result.OrderBy(x => x.QuestionResponses) : result.OrderByDescending(x => x.QuestionResponses);
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


        [HttpDelete]
        [Authorize]
        [Route("question/{guid}")]
        public async Task<IActionResult> DeleteQuestion(string guid)
        {
            try
            {
                var response = await formService.DeleteQuestion(Guid.Parse(guid));
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Question DELETE - {guid}",
                    UserId = userId,
                    EntityId = response.Id,
                    EntityName = "Question",
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
        [Authorize]
        [Route("question/{guid}")]
        public IActionResult QuestionDetails(string guid)
        {

            try
            {
                var authorization = Request.Headers.Authorization.ToString();

                try
                {

                    return Ok(formService.GetQuestionDetails(Guid.Parse(guid), null));
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
        [Authorize]
        [Route("question")]
        public async Task<IActionResult> SaveQuestion(FormModel.QuestionRequest request)
        {
            try
            {
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
                var result = await formService.SaveQuestion(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = !string.IsNullOrEmpty(request.Guid) ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = !string.IsNullOrEmpty(request.Guid) ? $"Question UPDATE - {result}" : $"Form CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Question",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id, result.Guid } });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message} | EX : {ex.InnerException}");
            }
        }
        #endregion Question

        #region  Option
        [HttpPost]
        [Authorize]
        [Route("option")]
        public async Task<IActionResult> AddOption(FormModel.OptionRequest request)
        {
            try
            {
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
                var result = await formService.AddOption(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("create"),
                    Comment = $"Option CREATE - {result}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Option",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = new { result.Id } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Authorize]
        [Route("option/{id:int}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            try
            {
                var response = await formService.DeleteOption(id);
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"Option DELETE - {id}",
                    UserId = userId,
                    EntityId = response.Id,
                    EntityName = "Option",
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
        #endregion Option


        #region Mapping
        [HttpPost]
        [Authorize]
        [Route("MapQuestion")]
        public async Task<IActionResult> MapQuestion([FromBody] FormModel.FormQuestionMappingRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await formService.UpdateFormQuestionMapping(request);

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Action == UserModel.MethodType.insert
                    ? activityService.GetActivityLogType("create")
                    : request.Action == UserModel.MethodType.delete
                    ? activityService.GetActivityLogType("delete")
                    : activityService.GetActivityLogType("edit"),
                    Comment = $"Form Question Mapping - {result.formId} | {result.questionId}",
                    UserId = userId,
                    EntityId = result.formId,
                    EntityName = "Form",
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
