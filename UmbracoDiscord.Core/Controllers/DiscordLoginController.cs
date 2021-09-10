using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Services;

namespace UmbracoDiscord.Core.Controllers
{
    public class DiscordLoginController : RenderController
    {
        private readonly IDiscordAuthService _discordAuthService;

        public DiscordLoginController(ILogger<DiscordLoginController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,
            IDiscordAuthService discordAuthService) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _discordAuthService = discordAuthService;
        }

        public override IActionResult Index()
        { 

            var settings = (CurrentPage as DiscordLogin).Ancestor<DiscordSection>();
            if (settings == null)
            {
                return base.Index();
            }

            if (settings.ClientId.IsNullOrWhiteSpace() || settings.ClientId.IsNullOrWhiteSpace())
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
                $"https://discord.com/api/oauth2/authorize?response_type=code&client_id={settings.ClientId}&scope=identify%20email%20guilds&state={state}&redirect_uri={redirectPage.Url(mode:UrlMode.Absolute)}&prompt=none");
        }
    }
}
