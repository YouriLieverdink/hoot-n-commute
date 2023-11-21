using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Corpool.AspNetCoreTenant;
using CorPool.BackEnd.Helpers.Jwt;
using CorPool.Mongo.DatabaseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace CorPool.BackEnd.Controllers {
    public class SeedController : AbstractApiController {
        public SeedController(Lazy<DatabaseContext> Database, Lazy<JwtUserManager> userManager, Lazy<IDistributedCache> cache) : base(Database, userManager, cache) { }

        public async Task<ActionResult<string>> Get() {
            // Clear Database
            await Database.Offers.DeleteManyAsync(s => true);
            await Database.Users.DeleteManyAsync(s => true);
            await Database.Tenants.DeleteManyAsync(s => true);

            // Insert tenants
            var rug = new Tenant {
                Identifier = "rug",
                Name = "University of Groningen"
            };
            var ah = new Tenant {
                Identifier = "ah",
                Name = "Albert Heijn"
            };

            var tenants = new List<Tenant> { rug, ah };
            await Database.Tenants.InsertManyAsync(tenants);

            // Insert users
            var volvo = new Vehicle { Brand = "Volvo", Model = "V70", Color = "Blue", Capacity = 5 };
            var tesla = new Vehicle { Brand = "Tesla", Model = "Model S", Color = "Pink", Capacity = 5 };
            var golf = new Vehicle { Brand = "Volkswagen", Model = "Golf", Color = "Red", Capacity = 2 };
            var touran = new Vehicle { Brand = "Volkswagen", Model = "Touran", Color = "Gray", Capacity = 9 };
            var gazelle = new Vehicle { Brand = "Gazelle", Model = "Omafiets", Color = "Black", Capacity = 2 };
            var swapfiets = new Vehicle { Brand = "Swapfiets", Model = "The one", Color = "Blue Tire", Capacity = 2 };
            var vanmoof = new Vehicle { Brand = "VanMoof", Model = "Too expensive", Color = "Defunct", Capacity = 0 };

            var rugProfiles = new List<(string, string, Vehicle, Tenant)>
            {
                ("arnold", "Arnold Meijster", volvo, rug),
                ("vasilios", "Vasilios Andrikopoulos", touran, rug),
                ("alexander", "Alexander Lazovik", tesla, rug),
                ("jorge", "Jorge Perez Parra", golf, rug),
                ("student", "You!", null, rug),
            };

            var ahProfiles = new List<(string, string, Vehicle, Tenant)>
            {
                ("student2", "You!", swapfiets, ah),
                ("friend", "Your friend", gazelle, ah),
                ("boss", "Your boss", vanmoof, ah),
            };

            var users = rugProfiles.Concat(ahProfiles).Select(p => new User {
                TenantId = p.Item4.Id,
                UserName = p.Item1,
                NormalizedUserName = p.Item1.ToUpper(),
                Email = $"{p.Item1}@corpooling.{p.Item4.Identifier}.com",
                FullName = p.Item2,
                Vehicles = new List<Vehicle> { p.Item3 }
            }).ToList();

            await Database.Users.InsertManyAsync(users);

            // Set users passwords and emails
            foreach (var user in users) {
                // Temporarily override the current tenant, to allow the normal user store to
                // do tenant-specific stuff while setting up the users.
                HttpContext.Items[TenantMiddleware<ITenant>.ContextKey] = tenants.FirstOrDefault(s => s.Id == user.TenantId);
                await UserManager.AddPasswordAsync(user, "password");
                await UserManager.SetEmailAsync(user, user.Email);
            }

            return "Seeding Database succeeded";
        }
    }
}
