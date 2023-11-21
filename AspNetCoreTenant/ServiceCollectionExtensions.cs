using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Corpool.AspNetCoreTenant {
    /**
     * This class contains some helper methods for registering Tenant services
     * to the application.
     */
    public static class ServiceCollectionExtensions {
        /**
         * Add Tenant support to the application using the given tenant entity
         * type TTenant, and the given resolve function to map tenant names
         * to entities.
         */
        public static IServiceCollection AddTenanted<TTenant>(this IServiceCollection services, Func<string, Task<TTenant>> resolveAsync)
            where TTenant : class, ITenant {
            services.AddSingleton<ITenantAccessor, TenantAccessor>();
            services.AddSingleton<ITenantAccessor<TTenant>, TenantAccessor<TTenant>>();
            services.AddSingleton<ITenantResolver<TTenant>>(_ => new TenantResolver<TTenant>(resolveAsync));

            return services;
        }

        /**
         * Add Tenant support to the application using the given tenant entity
         * type TTenant, and the given TenantResolver implementation TResolver.
         */
        public static IServiceCollection AddTenanted<TTenant, TResolver>(this IServiceCollection services)
            where TTenant : class, ITenant where TResolver : class, ITenantResolver<TTenant> {
            services.AddSingleton<ITenantAccessor, TenantAccessor>();
            services.AddSingleton<ITenantAccessor<TTenant>, TenantAccessor<TTenant>>();
            services.AddSingleton<ITenantResolver<TTenant>, TResolver>();

            return services;
        }

        /**
         * Add Tenant-specific authentication support to the application
         */
        public static IServiceCollection AddTenantAuth<TOptions>(this IServiceCollection services)
            where TOptions : class, ITenantAuthOptions {
            services.AddSingleton<IAuthorizationHandler, TenantAuthorizationHandler<TOptions>>();

            return services;
        }

        /**
         * Use the Tenant middleware to actually activate the multitenancy
         */
        public static IApplicationBuilder UseTenanted<TTenant>(this IApplicationBuilder app)
            where TTenant : class, ITenant {
            app.UseMiddleware<TenantMiddleware<TTenant>>();

            return app;
        }
    }
}
