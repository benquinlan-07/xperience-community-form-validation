using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace XperienceCommunity.FormValidation.Middleware
{
    public class FormValidationExtensionMiddleware
    {
        private readonly RequestDelegate _next;
        private static string[] _matchPaths;
        private static string[] _excludedPaths;

        public FormValidationExtensionMiddleware(RequestDelegate next)
        {
            _next = next;
            if (_matchPaths == null)
                _matchPaths = ["/admin"];
            if (_excludedPaths == null)
                _excludedPaths = ["/admin/styles.css", "/admin/adminresources/", "/admin/api/"];
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (context.Request?.Path.Value != null && _matchPaths.Any(x => context.Request.Path.Value.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)) && !_excludedPaths.Any(x => context.Request.Path.Value.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
            {
                var originalBody = context.Response.Body;

                // Read out the body
                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;

                    await _next(context);

                    if (context.Response?.ContentType != null && context.Response.ContentType.StartsWith("text/html"))
                    {
                        memStream.Position = 0;
                        string responseBody = new StreamReader(memStream).ReadToEnd();

                        var styles = @"<style>.ReactModalPortal > [class^=overlay__] {z-index:3000;}</style>";
                        responseBody = responseBody.Replace("</head>", $"{styles}</head>");

                        using (var newStream = new MemoryStream())
                        using (var streamWriter = new StreamWriter(newStream))
                        {
                            streamWriter.Write(responseBody);
                            streamWriter.Flush();

                            newStream.Position = 0;
                            await newStream.CopyToAsync(originalBody);
                        }
                    }
                    else
                    {
                        memStream.Position = 0;
                        await memStream.CopyToAsync(originalBody);
                    }
                }

                return;
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
    public static class FormValidationExtensionMiddlewareExtensions
    {
        public static IApplicationBuilder UseFormValidationExtensionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FormValidationExtensionMiddleware>();
        }
    }
}
