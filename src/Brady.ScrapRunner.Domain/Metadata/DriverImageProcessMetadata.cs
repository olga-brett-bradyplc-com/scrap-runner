using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverImageProcessMetadata : TypeMetadataProvider<DriverImageProcess>
    {
        public DriverImageProcessMetadata()
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
            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.PrintedName);
            StringProperty(x => x.ImageType);
            ImageProperty(x => x.ImageByteArray);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.ActionDateTime)
            .Property(x => x.PrintedName)
            .Property(x => x.ImageType)
            .Property(x => x.ImageByteArray)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
