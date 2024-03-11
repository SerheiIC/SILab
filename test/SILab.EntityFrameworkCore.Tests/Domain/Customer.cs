using SILab.Domain.Entities.Auditing;
using SILab.Domain.Entities;


namespace SILab.EntityFrameworkCore.Tests.Domain
{
    public class Customer : AuditedEntity<Guid>, ISoftDelete
    { 
        public required string Name { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Address> Addresses { get; set; }

        public Customer() 
        {
            Id = Guid.NewGuid();
            Addresses = new List<Address>();
        }

        public Customer(string name) : this()
        {
            Name = name;
            Addresses = new List<Address>();
        }
    }
}
