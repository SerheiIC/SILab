using Microsoft.EntityFrameworkCore;
using SILab.EntityFrameworkCore.Tests.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SILab.EntityFrameworkCore.Tests.Ef
{
    public class AdventureWorksDbContext : SILabDbContext
    { 
        public DbSet<Address> Addresses { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public AdventureWorksDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
