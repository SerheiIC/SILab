using SILab.EntityFramework.Tests.Domain;
using System.Data.Entity;


namespace SILab.EntityFramework.Tests.Ef
{
    public class AutoPartsDbContext : SILabDbContext
    { 
        public DbSet<PartsMovementTransfer> PartsMovementTransfers { get; set; }

        public DbSet<Sale> Sales { get; set; }
         
    }
}
