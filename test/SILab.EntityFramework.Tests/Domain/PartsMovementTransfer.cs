using SILab.Domain.Entities;
using SILab.Domain.Entities.Auditing;

namespace SILab.EntityFramework.Tests.Domain
{
    public class PartsMovementTransfer : AggregateRoot, IHasCreationTime
    {
        public string TrackingNumber { get; protected set; }
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public string PartNumber { get; set; }
        public string OriginPlace { get; set; }
        public string Destination { get; set; }
        public DateTime CreationTime { get; set; }

        public ICollection<Sale> Sales { get; set; }

        public PartsMovementTransfer() { }

        public PartsMovementTransfer(string trackingNumber, string invoiceNumber, string partNumber)
        {
            TrackingNumber = trackingNumber; 
            InvoiceNumber = invoiceNumber;
            PartNumber = partNumber;
        }

        public void ChangeTrackingNumber(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                throw new ArgumentNullException(nameof(trackingNumber));
            }

            var oldTrackingNumber = TrackingNumber;
            TrackingNumber = trackingNumber;

            DomainEvents.Add(new TrackingNumberChangedEventData(this, oldTrackingNumber));
        }
    }
}
