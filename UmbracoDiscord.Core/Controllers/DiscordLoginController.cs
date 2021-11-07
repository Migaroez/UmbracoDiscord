using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Constants;
using UmbracoDiscord.Core.Services;

namespace UmbracoDiscord.Core.Controllers
{
    public class DiscordLoginController : RenderController
    {
        private readonly IDiscordService _discordAuthService;
        private readonly IConfiguration _configuration;

        public DiscordLoginController(ILogger<DiscordLoginController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,
            IDiscordService discordAuthService, IConfiguration configuration) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _discordAuthService = discordAuthService;
            _configuration = configuration;
        }

        public override IActionResult Index()
        { 

            var settings = (CurrentPage as DiscordLogin).Ancestor<DiscordSection>();
            if (settings == null)
            {
                return base.Index();
            }

            if (_configuration["Discord:ClientId"].IsNullOrWhiteSpace() || _configuration["Discord:ClientSecret"].IsNullOrWhiteSpace())
            {
                return base.Index();
            }

            var state = _discordAuthService.GetState(HttpContext, true);
            if (state == null)
            {
                return base.Index();
            }
            var redirectPage = settings.FirstChild<DiscordLoginRedirectHandler>();


            return Redirect(
                $"{DiscordApi.AuthorizeEndpoint}?response_type=code&client_id={_configuration["Discord:ClientId"]}&scope=identify%20email%20guilds&state={state}&redirect_uri={redirectPage.Url(mode:UrlMode.Absolute)}&prompt=none");
        }
    }
}
