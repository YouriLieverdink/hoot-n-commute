using System.Collections.Generic;
using System.Linq;
using DatabaseModels = CorPool.Mongo.DatabaseModels;

namespace CorPool.Shared.ApiModels {
    public class User {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<Vehicle> Vehicles { get; set; }


        public User() { }
        public User(DatabaseModels.User user) {
            Id = user.Id;
            FullName = user.FullName;
            UserName = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            Vehicles = user.Vehicles?.Where(s => s != null)?.Select(s => new Vehicle(s))?.ToList() ?? new List<Vehicle>();
        }
    }
}
