using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverTripAckProcessMetadata : TypeMetadataProvider<DriverTripAckProcess>
    {
        public DriverTripAckProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.TripNumber);
            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.Mdtid);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.TripNumber)
            .Property(x => x.ActionDateTime)
            .Property(x => x.Mdtid)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
