# Next.js + Umbraco Delivery API + Preview Mode

This repo is an update to the [official Next.js Umbraco example](https://github.com/vercel/next.js/tree/canary/examples/cms-umbraco)
and the (accompanying Umbraco)[https://github.com/kjac/NextJsUmbracoExample] example repos.

It includes a controller hijack the built-in umbraco preview rendering page and redirect you (securely) to the associated page on the Next.JS site in preview mode.

To get running, you need to configure your environment variables.

For Umbraco, add the following to your `appsettings.Development.json`:

```
"Preview": {
    "PreviewSecret": "preview-secret",
    "FrontendUrl": "http://localhost:3000/"
}
```

For Next.JS, copy the `.env.local.example` to `.env.local` and add the keys:

```
# Add your Umbraco server URL here. Please do not include a trailing slash.
UMBRACO_SERVER_URL=https://localhost:44390

# Add your Umbraco Delivery API key here if you want to use preview.
UMBRACO_DELIVERY_API_KEY=super-secret-api-key

# Add the secret token that will be used to "authorize" preview
UMBRACO_PREVIEW_SECRET=preview-secret
```
