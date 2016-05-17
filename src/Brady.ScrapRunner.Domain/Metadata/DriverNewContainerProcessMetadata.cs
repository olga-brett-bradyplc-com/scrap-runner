using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverNewContainerProcessMetadata : TypeMetadataProvider<DriverNewContainerProcess>
    {
        public DriverNewContainerProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.ContainerNumber);
            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            StringProperty(x => x.ContainerBarcode);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.ActionDateTime)
            .Property(x => x.ContainerNumber)
            .Property(x => x.ContainerType)
            .Property(x => x.ContainerSize)
            .Property(x => x.ContainerBarcode)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
