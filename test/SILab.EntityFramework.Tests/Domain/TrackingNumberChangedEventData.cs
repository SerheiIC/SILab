using SILab.Events.Bus;

namespace SILab.EntityFramework.Tests.Domain
{
    internal class TrackingNumberChangedEventData : EventData
    {
        public PartsMovementTransfer PartsMovementTransfer { get; private set; }

        public string TrackingNumber { get; private set; }

        public TrackingNumberChangedEventData(PartsMovementTransfer partsMovementTransfer,
                                              string oldTrackingNumber) 
        {
            PartsMovementTransfer = partsMovementTransfer;
            TrackingNumber = oldTrackingNumber;
        }
        
    }
}
