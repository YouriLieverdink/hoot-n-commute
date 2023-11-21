using System;
using System.Threading.Tasks;

namespace Corpool.AspNetCoreTenant {
    /**
     * This interface represents a tenant resolver. A tenant resolver serves as a simple
     * lookup service, that given a tenant identifier (such as a subdomain name), will
     * find and retrieve the associated tenant entity.
     */
    public interface ITenantResolver<TTenant> where TTenant : ITenant {
        Task<TTenant> ResolveTentantAsync(string identifier);
    }

    /**
     * This is a default implementation of a tenant resolver that will just take
     * a simple lamdba function as lookup. This suffices for simple testing implementations,
     * for more elaborate implementations it is recommended to create a custom
     * implementation of the ITenantResolver interface. That way, the full DI solution
     * can be leveraged.
     */
    internal class TenantResolver<TTenant> : ITenantResolver<TTenant> where TTenant : class, ITenant {
        private readonly Func<string, Task<TTenant>> _resolveAsync;

        public TenantResolver(Func<string, Task<TTenant>> resolveAsync) {
            _resolveAsync = resolveAsync;
        }

        public Task<TTenant> ResolveTentantAsync(string identifier) => _resolveAsync(identifier);
    }
}
