using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverDelayMetadata : TypeMetadataProvider<DriverDelay>
    {
        public DriverDelayMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.DriverId)
                .IsId()
                .DisplayName("Driver Id");

            IntegerProperty(x => x.DelaySeqNumber)
                .IsId()
                .DisplayName("Delay Seq Number");

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber);
            StringProperty(x => x.DriverName);
            StringProperty(x => x.DelayCode);
            StringProperty(x => x.DelayReason);
            DateProperty(x => x.DelayStartDateTime);
            DateProperty(x => x.DelayEndDateTime);
            IntegerProperty(x => x.DelayLatitude);
            IntegerProperty(x => x.DelayLongitude);
            StringProperty(x => x.TerminalId);
            StringProperty(x => x.RegionId);
            StringProperty(x => x.TerminalName);
            StringProperty(x => x.RegionName);

            ViewDefaults()
                .Property(x => x.DriverId)
                .Property(x => x.DelaySeqNumber)
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.DriverName)
                .Property(x => x.DelayCode)
                .Property(x => x.DelayReason)
                .Property(x => x.DelayStartDateTime)
                .Property(x => x.DelayEndDateTime)
                .Property(x => x.DelayLatitude)
                .Property(x => x.DelayLongitude)
                .Property(x => x.TerminalId)
                .Property(x => x.RegionId)
                .Property(x => x.TerminalName)
                .Property(x => x.RegionName)

                .OrderBy(x => x.DriverId)
                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.DelaySeqNumber);

        }
    }
}
