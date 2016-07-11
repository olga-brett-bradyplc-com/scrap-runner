using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverLogoffProcessMetadata : TypeMetadataProvider<DriverLogoffProcess>
    {
        public DriverLogoffProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.PowerId);
            IntegerProperty(x => x.Odometer);
            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.TripNumber);
            StringProperty(x => x.TripSegNumber);
            StringProperty(x => x.DriverStatus);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.PowerId)
            .Property(x => x.Odometer)
            .Property(x => x.ActionDateTime)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.DriverStatus)
            .OrderBy(x => x.EmployeeId);
        }
    }
}
