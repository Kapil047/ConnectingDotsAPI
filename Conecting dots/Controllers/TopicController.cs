using Azure.Core;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services.HelperService;
using ConnectingDotsAPI.Services.TopicsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TopicController(ITopicsService topicsService, IHelperService helperService, IActivityService activityService, IAuthService authService, IHttpContextAccessor httpContextAccessor, ICacheService cacheService) : ControllerBase
    {
        private readonly ITopicsService topicsService = topicsService;
        private readonly IHelperService helperService = helperService;
        private readonly IActivityService activityService = activityService;
        private readonly IAuthService authService = authService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly ICacheService cacheService = cacheService;

        [HttpPost]
        [Route("records")]
        public async Task<IActionResult> GetAll([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);


                string cacheKey = $"topic_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<TopicModel.TopicDetails>();
                if (!string.IsNullOrEmpty(value))
                {
                    _result = JsonConvert.DeserializeObject<List<TopicModel.TopicDetails>>(value);
                }
                else
                {
                    _result = await topicsService.GetTopics();
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
                    case "systemName":
                        result = result.Where(x => x.SystemName != null && x.SystemName.ToLower().Contains(searchValue));
                        break;
                    case "title":
                        result = result.Where(x => x.Title != null && x.Title.ToLower().Contains(searchValue));
                        break;
                    case "body":
                        result = result.Where(x => x.Body != null && x.Body.ToLower().Contains(searchValue));
                        break;
                    case "metaKeywords":
                        result = result.Where(x => x.MetaKeywords != null && x.MetaKeywords.ToLower().Contains(searchValue));
                        break;
                    case "metaDescription":
                        result = result.Where(x => x.MetaDescription != null && x.MetaDescription.ToLower().Contains(searchValue));
                        break;
                    case "metaTitle":
                        result = result.Where(x => x.MetaTitle != null && x.MetaTitle.ToLower().Contains(searchValue));
                        break;
                    case "slug":
                        result = result.Where(x => x.Slug != null && x.Slug.ToLower().Contains(searchValue));
                        break;
                    case "displayOrder":
                        result = result.Where(x => x.DisplayOrder.ToString().Contains(searchValue));
                        break;
                    case "published":
                        result = result.Where(x => x.Published.ToString().ToLower().Contains(searchValue));
                        break;
                    case "isPasswordProtected":
                        result = result.Where(x => x.IsPasswordProtected.ToString().ToLower().Contains(searchValue));
                        break;
                    case "includeInTopMenu":
                        result = result.Where(x => x.IncludeInTopMenu.ToString().ToLower().Contains(searchValue));
                        break;
                    case "includeInFooterColumn1":
                        result = result.Where(x => x.IncludeInFooterColumn1.ToString().ToLower().Contains(searchValue));
                        break;
                    case "includeInFooterColumn2":
                        result = result.Where(x => x.IncludeInFooterColumn2.ToString().ToLower().Contains(searchValue));
                        break;
                    case "includeInFooterColumn3":
                        result = result.Where(x => x.IncludeInFooterColumn3.ToString().ToLower().Contains(searchValue));
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
                        case "systemName":
                            result = direction == "asc" ? result.OrderBy(x => x.SystemName) : result.OrderByDescending(x => x.SystemName);
                            break;
                        case "title":
                            result = direction == "asc" ? result.OrderBy(x => x.Title) : result.OrderByDescending(x => x.Title);
                            break;
                        case "displayOrder":
                            result = direction == "asc" ? result.OrderBy(x => x.DisplayOrder) : result.OrderByDescending(x => x.DisplayOrder);
                            break;
                        case "published":
                            result = direction == "asc" ? result.OrderBy(x => x.Published) : result.OrderByDescending(x => x.Published);
                            break;
                        case "isPasswordProtected":
                            result = direction == "asc" ? result.OrderBy(x => x.IsPasswordProtected) : result.OrderByDescending(x => x.IsPasswordProtected);
                            break;
                        case "includeInFooterColumn1":
                            result = direction == "asc" ? result.OrderBy(x => x.IncludeInFooterColumn1) : result.OrderByDescending(x => x.IncludeInFooterColumn1);
                            break;
                        case "includeInFooterColumn2":
                            result = direction == "asc" ? result.OrderBy(x => x.IncludeInFooterColumn2) : result.OrderByDescending(x => x.IncludeInFooterColumn2);
                            break;
                        case "includeInFooterColumn3":
                            result = direction == "asc" ? result.OrderBy(x => x.IncludeInFooterColumn3) : result.OrderByDescending(x => x.IncludeInFooterColumn3);
                            break;
                        case "includeInTopMenu":
                            result = direction == "asc" ? result.OrderBy(x => x.IncludeInTopMenu) : result.OrderByDescending(x => x.IncludeInTopMenu);
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
        [Authorize]
        public async Task<IActionResult> SaveTopic(TopicModel.TopicRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                var result = await topicsService.SaveTopic(request);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = request.Id.HasValue ? activityService.GetActivityLogType("update") : activityService.GetActivityLogType("create"),
                    Comment = request.Id.HasValue ? $"TOPIC UPDATE - {result}" : $"TOPIC CREATE - {result}",
                    UserId = userId,
                    EntityId = result,
                    EntityName = "Topic",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("{id:int}"), HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteTopic([FromBody] int id)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);
                await topicsService.DeleteTopic(id);
                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"TOPIC DELETE - {id}",
                    UserId = userId,
                    EntityId = id,
                    EntityName = "Topic",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTopics(string? f)
        {
            try
            {
                var result = await topicsService.GetTopics();

                if (!string.IsNullOrEmpty(f))
                {
                    if (f == "menu")
                    {
                        return Ok(
                            (await topicsService.GetTopics()).Where(x => x.IncludeInTopMenu).Select(x => new
                            {
                                x.Title,
                                x.Slug
                            })
                            );
                    }
                    else if (f == "footer")
                    {
                        return Ok(
                            (await topicsService.GetTopics()).Where(x => x.IncludeInFooterColumn1
                || x.IncludeInFooterColumn2
                || x.IncludeInFooterColumn3).Select(x => new
                {
                    x.Title,
                    x.Slug,
                    x.IncludeInFooterColumn1,
                    x.IncludeInFooterColumn2,
                    x.IncludeInFooterColumn3
                })
                            );
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("{id:int}"), HttpPut]
        public IActionResult GetTopicDetails(int id)
        {
            try
            {
                var result = topicsService.GetTopicDetails(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("{slug}"), HttpGet]
        public IActionResult GetTopicDetailsBySlug(string slug)
        {
            try
            {
                var id = helperService.GetEntityId("Topic", slug);
                if (id != null)
                {
                    var res = topicsService.GetTopicDetails(id.Value);
                    return Ok(res);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
