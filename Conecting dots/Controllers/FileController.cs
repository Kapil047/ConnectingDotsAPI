using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.ActivityService;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.AzureService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using ConnectingDotsAPI.Services.FileService;
using ConnectingDotsAPI.Services.AwsService;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController(IAzureService azureService, IAuthService authService, IFileService fileService, IActivityService activityService, IHttpContextAccessor httpContextAccessor, IAWSService awsService) : ControllerBase
    {
        private readonly IAzureService azureService = azureService;
        private readonly IAuthService authService = authService;
        private readonly IActivityService activityService = activityService;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly IFileService fileService = fileService;
        private readonly IAWSService awsService = awsService;
        [Produces("application/json")]
        [HttpPost, Route("Upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadBlobFile()
        {
            try
            {
                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();

                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName?.Trim('"');
                    if (fileName != null)
                    {
                        var result = await awsService.UploadBlob(fileName, fileBytes, true);
                        //var result = await azureService.UploadBlob(fileBytes, fileName, true);

                        return Ok(result);
                    }
                    return BadRequest();

                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        [HttpPost, Route("AddDownload")]
        public async Task<IActionResult> AddDownload(FileModel.DownloadUploadRequest request)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);

                var result = await fileService.SaveDownload(request);
                if (result == null)
                {
                    return BadRequest();
                }

                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"DOWNLOAD UPLOAD - {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "Class",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }
        [Route("{guid}")]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid guid)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);

                var result = await fileService.DeleteDownload(guid);


                await activityService.LogActivity(new ActivityLogModel.LogActivityRequest
                {
                    ActivityLogTypeId = activityService.GetActivityLogType("delete"),
                    Comment = $"FILE DELETE - {result.Id}",
                    UserId = userId,
                    EntityId = result.Id,
                    EntityName = "File",
                    IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                });
                return Ok(new { result = true, message = result });
            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }
    }
}
