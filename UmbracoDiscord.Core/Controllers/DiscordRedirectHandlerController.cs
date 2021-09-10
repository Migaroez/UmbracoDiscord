using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Services;

namespace UmbracoDiscord.Core.Controllers
{
    public class DiscordLoginRedirectHandlerController : RenderController
    {
        private readonly ILogger<DiscordLoginRedirectHandlerController> _logger;
        private readonly IDiscordAuthService _discordAuthService;

        public DiscordLoginRedirectHandlerController(ILogger<DiscordLoginRedirectHandlerController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,
            IDiscordAuthService discordAuthService) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
            _discordAuthService = discordAuthService;
        }

        //todo figure out why override doesnt work with async
        public override IActionResult Index()
        {
            if (_discordAuthService.IsValidState(HttpContext) == false)
            {
                _logger.LogInformation("Discord redirect handling failed: Invalid state");
                return base.Index();
            }

            if (_discordAuthService.HandleRedirect(HttpContext,CurrentPage.Ancestor<DiscordSection>()).GetAwaiter().GetResult() == false)
            {
                _logger.LogInformation("DiscordAuthService failed to handle redirect");
                return base.Index();
            }

            return base.Redirect(CurrentPage.AncestorOrSelf(1).Url());
        }
    }
}
