using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RestRestaurant
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [DataMember(Name="password")]
        public string Password { get; set; }
        [DataMember(Name = "group")]
        public string Role { get; set; }

        public User(int id, string name, string email, string password, string role)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Role = role;
        }

        public override string ToString()
        {
            return $"<User> {Email} : {Name}";
        }
    }
}