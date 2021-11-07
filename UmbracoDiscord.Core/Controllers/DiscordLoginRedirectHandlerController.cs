using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;
using UmbracoDiscord.Core.Services;

namespace UmbracoDiscord.Core.Controllers
{
    public class DiscordLoginRedirectHandlerController : RenderController
    {
        private readonly ILogger<DiscordLoginRedirectHandlerController> _logger;
        private readonly IDiscordService _discordAuthService;
        private readonly IMemberManager _memberManager;
        private readonly IMemberSignInManager _memberSignInManager;

        public DiscordLoginRedirectHandlerController(ILogger<DiscordLoginRedirectHandlerController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,
            IDiscordService discordAuthService,
            IMemberManager memberManager,
            IMemberSignInManager memberSignInManager) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
            _discordAuthService = discordAuthService;
            _memberManager = memberManager;
            _memberSignInManager = memberSignInManager;
        }

        //todo figure out why override doesn't work with async
        public override IActionResult Index()
        {
            if (_discordAuthService.IsValidState(HttpContext) == false)
            {
                _logger.LogInformation("Discord redirect handling failed: Invalid state");
                return base.Index();
            }

            var handleRedirectResult = _discordAuthService
                .HandleRedirect(HttpContext, CurrentPage.Ancestor<DiscordSection>()).GetAwaiter().GetResult();
            if (handleRedirectResult.Success == false)
            {
                _logger.LogError(handleRedirectResult.Exception, "DiscordAuthService failed to handle redirect");
                return base.Index();
            }

            var memberIdentity = _memberManager.FindByEmailAsync(handleRedirectResult.Result).GetAwaiter().GetResult();
            _memberSignInManager.SignInAsync(memberIdentity, true, "discord");

            return base.Redirect(CurrentPage.AncestorOrSelf(1).Url());
        }
    }
}
