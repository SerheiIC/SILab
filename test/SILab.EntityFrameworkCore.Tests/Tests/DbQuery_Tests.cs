using Microsoft.Extensions.DependencyInjection;
using SILab.Domain.Repositories;
using SILab.EntityFrameworkCore.Tests.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace SILab.EntityFrameworkCore.Tests.Tests
{
    public class DbQuery_Tests : EntityFrameworkCoreModuleTestBase
    {
        [Fact]
        public async Task DbQuery_Test()
        {
            var addressRepository = _serviceProvider.GetService<IRepository<Address>>();
            var adresses = await addressRepository.GetAllListAsync();
            adresses.ShouldNotBeNull();
          
        }
    }
}
