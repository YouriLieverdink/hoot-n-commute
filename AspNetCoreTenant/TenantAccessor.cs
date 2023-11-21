using Microsoft.AspNetCore.Http;

namespace Corpool.AspNetCoreTenant {
    /**
     * Helper interface, a non-generic version of the ITenantAccessor.
     * This non-generic version allows the use of Dependency Injection for
     * implementations.
     */
    public interface ITenantAccessor : ITenantAccessor<ITenant> {}

    /**
     * This defines a Tenant Accessor - a class that will be able to
     * "find out" the current tenant from anywhere in the application.
     */
    public interface ITenantAccessor<out TTenant> where TTenant : ITenant {
        TTenant Tenant { get; }
    }

    /**
     * Default accessor implementation, returning just the Tenant interface,
     * and not a particular implementation type.
     */
    public class TenantAccessor : TenantAccessor<ITenant>, ITenantAccessor {
        public TenantAccessor(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }
    }

    /**
     * Default Tenant Accessor implementation, which will retrieve the current
     * Tenant as it is stored in the HttpContext.
     */
    public class TenantAccessor<TTenant> : ITenantAccessor<TTenant> where TTenant : class, ITenant {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantAccessor(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public TTenant Tenant => _httpContextAccessor.HttpContext.GetTenant<TTenant>();
    }
}
