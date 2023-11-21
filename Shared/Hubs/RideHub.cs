using System.Threading.Tasks;
using Corpool.AspNetCoreTenant;
using CorPool.Mongo.DatabaseModels;
using CorPool.Shared.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Core.DependencyInjection.Services;

namespace CorPool.Shared.Hubs {
    /**
     * This is a "Hub", essentially a "Controller" for SignalR (our RPC/RMI library).
     * This hub handles RPC code, and in this case will handle ride requests. It will
     * store those in the RabbitMQ queue. The Worker will then send messages back
     * to the Hub, that will be sent back to the user. This happens through the
     * so-called "backplane".
     */
    [Authorize]
    [Tenanted]
    public class RideHub : Hub<IRideHubClient> {
        private readonly IQueueService _queueService;
        private readonly IOptions<RabbitOptions> _queueOptions;
        private readonly DatabaseContext _database;

        private Tenant Tenant => Context.GetHttpContext().GetTenant<Tenant>();

        public RideHub(IQueueService queueService, IOptions<RabbitOptions> queueOptions, DatabaseContext database) {
            _queueService = queueService;
            _queueOptions = queueOptions;
            _database = database;
        }

        public async Task RideRequest(ApiModels.RideRequest request) {
            // Make a database model
            var dbRequest = new RideRequest {
                ArrivalTime = request.ArrivalTime,
                From = new Location { 
                    Description = request.From.Description,
                    Title = request.From.Title
                },
                To = new Location {
                    Description = request.To.Description,
                    Title = request.To.Title
                },
                UserId = Context.UserIdentifier,
                TenantId = Tenant.Id
            };

            // Store
            await _database.RideRequests.InsertOneAsync(dbRequest);

            // Queue
            var options = _queueOptions.Value;
            await _queueService.SendStringAsync(dbRequest.Id, options.Name, options.RoutingKey);
        }
    }
}
