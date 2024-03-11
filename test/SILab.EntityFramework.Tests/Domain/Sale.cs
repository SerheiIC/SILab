using SILab.Domain.Entities;
using SILab.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace SILab.EntityFramework.Tests.Domain
{
    public class Sale : AuditedEntity<Guid>, ISoftDelete
    {
        [Required]
        public PartsMovementTransfer PartsMovementTransfer { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime DateOfSale { get; set; }

        public string PartNumber { get; set; }

        public string CustomerNumber { get; set; }

        public bool IsDeleted { get; set; }

        public Sale() 
        {
            Id = Guid.NewGuid();
            InvoiceNumber = GenerateInvoice();               
        }

        public Sale(PartsMovementTransfer partsMovementTransfer,
            string partNumber,
            string customerNumber) : this()
        {
            PartsMovementTransfer = partsMovementTransfer;
            PartNumber = partNumber;
            CustomerNumber = customerNumber;
        }

        private string GenerateInvoice()
        {
            return
                $"{Guid.NewGuid().ToString("N").Substring(0, 8)}-{DateTime.Now.ToShortDateString}";
        }
    }
}
