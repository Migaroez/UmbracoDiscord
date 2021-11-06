using Umbraco.Cms.Web.Common.Controllers;

namespace UmbracoDiscord.Core
{
    public class TestController : UmbracoApiController
    {
        public bool Ping()
        {
            return true;
        }
    }
}
