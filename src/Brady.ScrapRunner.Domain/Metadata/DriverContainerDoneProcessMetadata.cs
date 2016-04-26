using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverContainerDoneProcessMetadata : TypeMetadataProvider<DriverContainerDoneProcess>
    {
        public DriverContainerDoneProcessMetadata()
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
            StringProperty(x => x.PowerId);
            IntegerProperty(x => x.Odometer);
            StringProperty(x => x.ContainerNumber);
            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            StringProperty(x => x.CommodityCode);
            StringProperty(x => x.CommodityDesc);
            StringProperty(x => x.ContainerLocation);
            StringProperty(x => x.ContainerContents);
            IntegerProperty(x => x.ContainerLevel);
            IntegerProperty(x => x.Latitude);
            IntegerProperty(x => x.Longitude);
            StringProperty(x => x.ActionType);
            StringProperty(x => x.ActionCode);
            StringProperty(x => x.ActionDesc);
            StringProperty(x => x.MethodOfEntry);

            StringProperty(x => x.ScaleReferenceNumber);
            StringProperty(x => x.SetInYardFlag);
            TimeProperty(x => x.Gross1ActionDateTime);
            IntegerProperty(x => x.Gross1Weight);
            TimeProperty(x => x.Gross2ActionDateTime);
            IntegerProperty(x => x.Gross2Weight);
            TimeProperty(x => x.TareActionDateTime);
            IntegerProperty(x => x.TareWeight);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.ActionDateTime)
            .Property(x => x.PowerId)
            .Property(x => x.Odometer)
            .Property(x => x.ContainerNumber)
            .Property(x => x.ContainerType)
            .Property(x => x.ContainerSize)
            .Property(x => x.CommodityCode)
            .Property(x => x.CommodityDesc)
            .Property(x => x.ContainerLocation)
            .Property(x => x.ContainerContents)
            .Property(x => x.ContainerLevel)
            .Property(x => x.Latitude)
            .Property(x => x.Longitude)
            .Property(x => x.ActionType)
            .Property(x => x.ActionCode)
            .Property(x => x.ActionDesc)
            .Property(x => x.MethodOfEntry)

            .Property(x => x.ScaleReferenceNumber)
            .Property(x => x.SetInYardFlag)
            .Property(x => x.Gross1ActionDateTime)
            .Property(x => x.Gross1Weight)
            .Property(x => x.Gross2ActionDateTime)
            .Property(x => x.Gross2Weight)
            .Property(x => x.TareActionDateTime)
            .Property(x => x.TareWeight)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
