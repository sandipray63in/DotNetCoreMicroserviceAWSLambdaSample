using Microsoft.EntityFrameworkCore;
using LoanMicroserviceLambda.Models;

namespace LoanMicroserviceLambda.EFContextsAndMaps
{
    public class LoanContext : DbContext
    {
        public LoanContext(DbContextOptions options):base(options)
        {

        }

        DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new LoanMap());
        }
    }
}
