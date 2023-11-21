using System;
using System.Threading.Tasks;
using Corpool.AspNetCoreTenant;
using CorPool.BackEnd.Helpers.Jwt;
using CorPool.Mongo.DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CorPool.BackEnd.Controllers {
    /**
     * This abstract API controller serves as a base controller implementation that
     * has some basic DI stuff arranged for any controller.
     */
    [ApiController]
    [Route("api/[controller]")]
    public abstract class AbstractApiController : ControllerBase {
        /**
         * Nearly all properties in this class are lazily loaded: we request DI
         * to inject a Lazy<T> instance, so not every request will open a database
         * connection AND a Redis connection AND find the current user in the database,
         * etc. Instead, these connections are only made when the service is actually
         * consumed, through the get-only properties.
         */

        private readonly Lazy<DatabaseContext> _database;
        protected DatabaseContext Database => _database.Value;

        protected Tenant Tenant => HttpContext.GetTenant<Tenant>();

        private readonly Lazy<JwtUserManager> _userManager;
        protected JwtUserManager UserManager => _userManager.Value;

        protected new Task<User> User => UserManager.GetUserAsync(base.User);
        protected string UserId => UserManager.GetUserId(base.User);

        private readonly Lazy<IDistributedCache> _cache;
        protected IDistributedCache Cache => _cache.Value;

        protected AbstractApiController(Lazy<DatabaseContext> database, Lazy<JwtUserManager> userManager, Lazy<IDistributedCache> cache) {
            _database = database;
            _userManager = userManager;
            _cache = cache;
        }
    }
}
