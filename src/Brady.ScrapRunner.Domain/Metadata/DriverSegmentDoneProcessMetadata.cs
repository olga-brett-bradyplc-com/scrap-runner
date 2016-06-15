using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverSegmentDoneProcessMetadata : TypeMetadataProvider<DriverSegmentDoneProcess>
    {
        public DriverSegmentDoneProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.TripNumber);
            StringProperty(x => x.TripSegNumber);
            StringProperty(x => x.ActionType);
            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.PowerId);
            IntegerProperty(x => x.Latitude);
            IntegerProperty(x => x.Longitude);
            StringProperty(x => x.DestCustHostCode);
            StringProperty(x => x.DriverGenerated);
            StringProperty(x => x.DriverModified);
            StringProperty(x => x.Mdtid);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.ActionType)
            .Property(x => x.ActionDateTime)
            .Property(x => x.PowerId)
            .Property(x => x.Latitude)
            .Property(x => x.Longitude)
            .Property(x => x.DestCustHostCode)
            .Property(x => x.DriverGenerated)
            .Property(x => x.DriverModified)
            .Property(x => x.Mdtid)

            .OrderBy(x => x.EmployeeId);
        }
    }
}