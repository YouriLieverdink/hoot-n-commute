using System;
using CorPool.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Core.DependencyInjection;

namespace CorPool.Worker {
    public class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(HostConfiguration.Host<Worker>(args))
                .ConfigureServices((hostContext, services) => {
                    // Configure services
                    services.ConfigureSharedServices(hostContext.Configuration);

                    // Add hosted service
                    services.AddHostedService<Worker>();

                    // Add RabbitMq Client
                    services.AddRabbitMqConsumer(hostContext.Configuration.GetSection("RabbitMq"))
                        .AddAsyncMessageHandlerSingleton<MessageHandler>("*");

                    // Health check options if you want

                    services.AddLazyLoading();
                });
    }
}
