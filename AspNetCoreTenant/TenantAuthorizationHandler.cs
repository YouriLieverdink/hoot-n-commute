using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Corpool.AspNetCoreTenant {
    /**
     * This is an implementation of an "Authorization Handler" for multitenancy.
     * An auth handler is called by the framework when a given identity must be
     * verified. This verification is done through a set of "Requirements", as
     * as specified in the application configuration. When one of these
     * requirements is our "TenantAuthorizationRequirement", this class will be
     * used to check the validity of the entity.
     */
    public class TenantAuthorizationHandler<TOptions> : AuthorizationHandler<TenantAuthorizationRequirement>
        where TOptions : ITenantAuthOptions {
        private readonly ITenantAccessor _tenantAccessor;
        private readonly IOptionsMonitor<TOptions> _authOptions;

        public TenantAuthorizationHandler(ITenantAccessor tenantAccessor, IOptionsMonitor<TOptions> authOptions) {
            _tenantAccessor = tenantAccessor;
            _authOptions = authOptions;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            TenantAuthorizationRequirement requirement) {
            // If no valid tenant is available to the accessor, we have failed
            // to meet the auth requirement, thus we fail.
            if (_tenantAccessor.Tenant == null) {
                context.Fail();
                return Task.CompletedTask;
            }

            // When we have found a tenant, we check whether the current user
            // does indeed belong to this tenant. We make sure that this claim
            // is signed by the current authority.
            if (context.User.HasClaim(s =>
                s.Type == "Tenant" &&
                s.Value == _tenantAccessor.Tenant.Id &&
                s.Issuer == _authOptions.CurrentValue.Authority
            )) {
                context.Succeed(requirement);
            } else {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
