using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Common.UmbracoContext;

namespace Site.Controllers
{



    public class PreviewRenderController : RenderController
    {


        private readonly PreviewConfig _config;
        private IPublishedUrlProvider _publishedUrlProvider;


        public PreviewRenderController(ILogger<RenderController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IPublishedUrlProvider publishedUrlProvider,
            IOptions<PreviewConfig> config)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this._config = config.Value;
            this._publishedUrlProvider = publishedUrlProvider;
        }

        private Dictionary<string, string> createHmacSignature(string path)
        {
            string expiryDateTime = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString();

            string stringToHash = $"{path}|{expiryDateTime}";
            byte[] dataAsBytes = Encoding.UTF8.GetBytes(stringToHash);
            string sig = "";

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.PreviewSecret)))
            {
                var hashBytes = hmac.ComputeHash(dataAsBytes);
                sig = Base64UrlEncoder.Encode(hashBytes);
            }

            return new Dictionary<string, string>()
            {
                { "message", stringToHash },
                { "expiry", expiryDateTime },
                { "path", path },
                { "sig", sig }
            };
        }

        [Route("/{id:int}")]
        public IActionResult Preview(int id, string culture)
        {
            var route = this._publishedUrlProvider.GetUrl(id, UrlMode.Relative, culture);
            return RedirectToPreview(route);
        }

        [Route("/{id:guid}")]
        public IActionResult Preview(Guid id, string culture)
        {
            var route = this._publishedUrlProvider.GetUrl(id, UrlMode.Relative, culture);
            return RedirectToPreview(route);
        }
        
        private IActionResult RedirectToPreview(string route)
        {
            if (!this.UmbracoContext.InPreviewMode)
            {
                Uri exitPathUrl = new Uri(new Uri(_config.FrontendUrl), _config.PreviewExitPath);
                return RedirectPreserveMethod(exitPathUrl.ToString());
            }

            if (String.IsNullOrEmpty(_config.PreviewSecret) ||
                String.IsNullOrEmpty(_config.PreviewPath))
                return BadRequest("Preview not configured");

            var queryParams = QueryString.Create(createHmacSignature(route));
            Uri previewUrl = new Uri(new Uri(_config.FrontendUrl), $"{_config.PreviewPath}{queryParams.Value}");

            return RedirectPreserveMethod(previewUrl.ToString());
        }
    }
}