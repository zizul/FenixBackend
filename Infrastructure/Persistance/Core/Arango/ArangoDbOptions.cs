namespace Infrastructure.Persistance.Core.Arango
{
    public class ArangoDbOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string HostUrl { get; set; }
        public string DbName { get; set; }
    }
}
