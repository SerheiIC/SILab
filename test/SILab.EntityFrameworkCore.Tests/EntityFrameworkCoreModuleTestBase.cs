using SILab.EntityFrameworkCore.Tests.Ef;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace SILab.EntityFrameworkCore.Tests
{
    public abstract class EntityFrameworkCoreModuleTestBase
    {
        private readonly ServiceCollection _serviceCollection;
        protected readonly ServiceProvider _serviceProvider;

        public EntityFrameworkCoreModuleTestBase() 
        { 
            _serviceCollection = new ServiceCollection();          
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            // Register AdventureWorksLiteDbContext 
            _serviceCollection.RegisterDbContextToSqliteInMemoryDb();
        }


        public void UsingDbContext(Action<AdventureWorksLiteDbContext> action)
        { 
            using (var context = _serviceProvider.CreateScope().ServiceProvider.GetService<AdventureWorksLiteDbContext>())
            {
                action(context);
                context.SaveChanges();
            }
        }
 
    }

    public static class ServiceCollectionExtensions
    {
        public static void RegisterDbContextToSqliteInMemoryDb(this ServiceCollection serviceCollection)
        {            
            var builder = new DbContextOptionsBuilder<AdventureWorksLiteDbContext>();

            var inMemorySqlite = new SqliteConnection("Data Source=:memory:");
            builder.UseSqlite(inMemorySqlite);

            serviceCollection.AddDbContext<AdventureWorksLiteDbContext>(options =>
            {
                
            });

            inMemorySqlite.Open();
            new AdventureWorksLiteDbContext(builder.Options).Database.EnsureCreated();
          
        }
    }


}
