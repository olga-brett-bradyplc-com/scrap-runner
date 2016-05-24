using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverFuelEntryProcessMetadata : TypeMetadataProvider<DriverFuelEntryProcess>
    {
        public DriverFuelEntryProcessMetadata()
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
            StringProperty(x => x.State);
            StringProperty(x => x.Country);
            NumericProperty(x => x.FuelAmount);
            StringProperty(x => x.Mdtid);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.ActionDateTime)
            .Property(x => x.PowerId)
            .Property(x => x.Odometer)
            .Property(x => x.State)
            .Property(x => x.Country)
            .Property(x => x.FuelAmount)
            .Property(x => x.Mdtid)

            .OrderBy(x => x.EmployeeId);
        }
    }
}

