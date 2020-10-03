using UserMicroserviceLambda.Models;
using Microsoft.EntityFrameworkCore;

namespace UserMicroserviceLambda.EFContextsAndMaps
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions options):base(options)
        {

        }

        DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMap());
        }
    }
}
