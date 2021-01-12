using System.Data.Entity;
using RealtyModel;


namespace RealtorClient.Model
{
    public class ClientContext : DbContext
    {
        public ClientContext() : base("ClientDBConnection")
        {
            Users.Load();
            Users.Local.CollectionChanged += (sender, e) => { this.SaveChanges(); };
        }
        public DbSet<User> Users { get; set; }
    }
}
