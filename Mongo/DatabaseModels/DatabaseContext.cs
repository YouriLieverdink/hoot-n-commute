using CorPool.Mongo.Providers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace CorPool.Mongo.DatabaseModels {
    /**
     * Class to provide easy access to queryables of data sets. This setup mimics
     * how this is typically done with Entity Framework for relational databases.
     */
    public class DatabaseContext {
        private MongoDbProvider _dbProvider;
        public DatabaseContext(MongoDbProvider dbProvider) {
            _dbProvider = dbProvider;
        }

        public IMongoCollection<Offer> Offers => _dbProvider.GetCollection<Offer>("Offers");
        public IMongoCollection<RideRequest> RideRequests => _dbProvider.GetCollection<RideRequest>("RideRequests");
        public IMongoCollection<Tenant> Tenants => _dbProvider.GetCollection<Tenant>("Tenants");
        public IMongoCollection<User> Users => _dbProvider.GetCollection<User>("Users");
    }

    public static class TenantedMongoCollectionExtensions {
        /**
         * This extension function allows any Mongo collection of "Tenanted" entities,
         * so entities that are tenant-specific, to be easily queried for the current
         * tenant without worrying about the specific query.
         */
        public static IMongoQueryable<T> Tenanted<T>(this IMongoCollection<T> collection, Tenant tenant)
            where T : ITenanted
            => collection.AsQueryable().Where(s => s.TenantId == tenant.Id);
    }
}
