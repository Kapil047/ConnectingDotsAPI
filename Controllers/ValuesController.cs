using ConnectingDotsAPI.Services.SettingsService;
using ConnectingDotsAPI.Services.CustomerService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ConnectingDotsAPI.Services.HelperService;

namespace ConnectingDotsAPI.Controllers
{
    [Route("/")]
    [Produces("application/json")]
    [ApiController]
    public class ValuesController(ISettingsService settingsService, IHelperService helperService) : ControllerBase
    {
        private readonly ISettingsService settingsService = settingsService;
        private readonly IHelperService helperService = helperService;
        [HttpGet]
        [Route("{name}")]
        public IActionResult Configuration(string name)
        {
            try
            {
                return Ok(settingsService.FindConfiguration(name));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Route("{key}")]
        public IActionResult SaveConfiguration(string key, [FromBody] string value)
        {
            try
            {
                settingsService.SaveConfiguration(new Models.SettingsModel.ConfigurationRequest { Name = key, Value = value });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("info")]
        public async Task<IActionResult> SystemConfigurations()
        {
            try
            {
                List<string> settings = ["text.color", "logo", "background.color", "mode", "favicon"];
                return Ok((await settingsService.GetAllSettings()).Where(x => settings.Contains(x.Name.ToLower())).Select(setting => new
                {
                    setting.Name,
                    setting.Value
                }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
