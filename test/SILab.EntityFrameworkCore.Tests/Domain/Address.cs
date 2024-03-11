using SILab.Domain.Entities;

namespace SILab.EntityFrameworkCore.Tests.Domain
{
    public class Address : Entity
    {
        public string AddressLine { get; set; }

        public string PostalCode { get; set; }

        public DateTime CreationTime { get; set; }

        public Customer Customer { get; set; }
               
              
    }
}
