using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverGPSLocationProcessMetadata : TypeMetadataProvider<DriverGPSLocationProcess>
    {
        public DriverGPSLocationProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            IntegerProperty(x => x.GPSID);
            TimeProperty(x => x.ActionDateTime);
            IntegerProperty(x => x.Latitude);
            IntegerProperty(x => x.Longitude);
            IntegerProperty(x => x.Speed);
            IntegerProperty(x => x.Heading);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.GPSID)
            .Property(x => x.ActionDateTime)
            .Property(x => x.Latitude)
            .Property(x => x.Longitude)
            .Property(x => x.Speed)
            .Property(x => x.Heading)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
