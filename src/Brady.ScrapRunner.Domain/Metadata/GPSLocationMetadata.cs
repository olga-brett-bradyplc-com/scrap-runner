using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using BWF.DataServices.Metadata.Fluent.Enums;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class GPSLocationMetadata : TypeMetadataProvider<GPSLocation>
    {
        public GPSLocationMetadata()
        {

            AutoUpdatesByDefault();

            IntegerProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            IntegerProperty(x => x.GPSSeqId)
                .IsId()
                .IsNotEditableInGrid()
                .DisplayName("GPS Seq Id");

            StringProperty(x => x.EmployeeId);
            StringProperty(x => x.TerminalId);
            StringProperty(x => x.RegionId);
            IntegerProperty(x => x.GPSID);
            TimeProperty(x => x.GPSDateTime);
            IntegerProperty(x => x.GPSLatitude);
            IntegerProperty(x => x.GPSLongitude);
            IntegerProperty(x => x.GPSSpeed);
            IntegerProperty(x => x.GPSHeading);
            EnumProperty(x => x.GPSSendFlag);

            ViewDefaults()
                .Property(x => x.GPSSeqId)
                .Property(x => x.EmployeeId)
                .Property(x => x.TerminalId)
                .Property(x => x.RegionId)
                .Property(x => x.GPSID)
                .Property(x => x.GPSDateTime)
                .Property(x => x.GPSLatitude)
                .Property(x => x.GPSLongitude)
                .Property(x => x.GPSSpeed)
                .Property(x => x.GPSHeading)
                .Property(x => x.GPSSendFlag)
                .OrderBy(x => x.GPSSeqId, Direction.Descending);
        }
    }
}
