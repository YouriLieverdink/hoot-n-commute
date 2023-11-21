using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CorPool.Shared.ApiModels;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Console = System.Console;

namespace CorPool.HubTest {
    class Program {
        static async Task Main(string[] args) {
            Console.WriteLine("Enter a server to connect to: (e.g. tenant.localhost:33080)");

            var url = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter your authorization key:");
            var token = Console.ReadLine()?.Trim();

            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{url}/api/ride/find",
                    HttpTransportType.WebSockets,
                    s => {
                        s.Headers.Add("Authorization", $"Bearer {token}");
                        s.SkipNegotiation = true;
                    })
                .WithAutomaticReconnect()
                .AddJsonProtocol()
                .Build();

            await hubConnection.StartAsync();
            hubConnection.On<Offer>(nameof(RideResult), RideResult);

            var end = false;
            while (!end) {
                Console.WriteLine("Enter your starting point");
                var start = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(start)) {
                    end = true;
                    continue;
                }

                Console.WriteLine("Enter your arrival time");
                var succeeded = DateTime.TryParse(Console.ReadLine(), out var arrivalTime);
                if (!succeeded) {
                    Console.WriteLine("Invalid date");
                    continue;
                }

                var request = new RideRequest {
                    ArrivalTime = arrivalTime,
                    From = new Location {
                        Title = start,
                        Description = ""
                    },
                    To = new Location {
                        Title = "",
                        Description = ""
                    }
                };

                // Send
                await hubConnection.SendAsync("RideRequest", request);
            }
        }

        static async Task RideResult(Offer offer) {
            if (offer == null) {
                Console.WriteLine("No result found!");
                return;
            }

            // Print offer
            Console.WriteLine("Offer found!");

            var printSettings = new JsonWriterSettings {
                OutputMode = JsonOutputMode.CanonicalExtendedJson,
                NewLineChars = "\n"
            };
            Console.Write(offer.ToJson(printSettings));
        }
    }
}
