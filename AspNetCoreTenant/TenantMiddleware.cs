using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Corpool.AspNetCoreTenant {
    /**
     * This middleware is run early during the request pipeline. It will determine the
     * current tenant based on the subdomain of the request. Then it will use the
     * tenant resolver to find the associated entity.
     */
    public class TenantMiddleware<TTenant> where TTenant : class, ITenant {
        public const string ContextKey = "Tenant";

        private readonly RequestDelegate _next;
        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, ITenantResolver<TTenant> resolver) {
            // Check whether there is a tenant already - if so, we do nothing.
            if (!context.Items.ContainsKey(ContextKey)) {

                // Find tenant identifier from the subdomain
                var identifier = context.Request.Host.Host.Split('.')[0];
                var tenant = await resolver.ResolveTentantAsync(identifier);
                context.Items.Add(ContextKey, tenant);

                // Check if endpoint has tenanted attribute and cancel request if so
                // Similar to what AuthZMiddleware does, bypassing the attribute filters :(
                var endpoint = context.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<TenantedAttribute>() != null && tenant == null) {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("No valid tenant specified");

                    return;
                }
            }

            if (_next != null) await _next(context);
        }
    }

    public static class HttpContextExtensions {
        public static TTenant GetTenant<TTenant>(this HttpContext context) where TTenant : class, ITenant {
            return context.Items[TenantMiddleware<ITenant>.ContextKey] as TTenant;
        }
    }
}
