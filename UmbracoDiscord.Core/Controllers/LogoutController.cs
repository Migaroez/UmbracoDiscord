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
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace UmbracoDiscord.Core.Controllers
{
    public class LogoutController : RenderController
    {
        private readonly IMemberSignInManager _memberSignInManager;

        public LogoutController(ILogger<LogoutController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,
            IMemberSignInManager memberSignInManager) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _memberSignInManager = memberSignInManager;
        }

        public override IActionResult Index()
        {
            _memberSignInManager.SignOutAsync().GetAwaiter().GetResult();

            return base.Redirect(CurrentPage.AncestorOrSelf(1).Url());
        }
    }
}
