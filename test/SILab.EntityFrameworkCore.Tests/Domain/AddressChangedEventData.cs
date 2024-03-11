using SILab.Events.Bus;

namespace SILab.EntityFrameworkCore.Tests.Domain
{
    public class AddressChangedEventData : EventData
    {
        public Address Address { get; private set; }
        public string OldAddressLine1 { get; private set; }
        public AddressChangedEventData(Address address, string oldAddressLine1) 
        {
            Address = address;
            OldAddressLine1 = oldAddressLine1;
        }
    }
}