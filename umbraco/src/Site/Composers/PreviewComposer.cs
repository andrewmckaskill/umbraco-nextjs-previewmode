using Umbraco.Cms.Core.Composing;

namespace Site.Composers;

public class PreviewComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<PreviewConfig>(options => builder.Config.Bind("Preview", options));
    }
}