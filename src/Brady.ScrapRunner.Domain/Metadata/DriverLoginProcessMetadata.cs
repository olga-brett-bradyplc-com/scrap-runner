using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverLoginProcessMetadata : TypeMetadataProvider<DriverLoginProcess>
    {
        public DriverLoginProcessMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.Password);
            StringProperty(x => x.PowerId);
            IntegerProperty(x => x.Odometer);
            TimeProperty(x => x.LoginDateTime);
            StringProperty(x => x.CodeListVersion);
            TimeProperty(x => x.LastContainerMasterUpdate);
            TimeProperty(x => x.LastTerminalMasterUpdate);
            StringProperty(x => x.LocaleCode);
            StringProperty(x => x.OverrideFlag);
            StringProperty(x => x.Mdtid);
            StringProperty(x => x.TermId);
            StringProperty(x => x.RegionId);
            StringProperty(x => x.AreaId);
            StringProperty(x => x.PndVer);
            StringProperty(x => x.TripNumber);
            StringProperty(x => x.TripSegNumber);
            StringProperty(x => x.DriverStatus);
            
            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.Password)
            .Property(x => x.PowerId)
            .Property(x => x.Odometer)
            .Property(x => x.LoginDateTime)
            .Property(x => x.CodeListVersion)
            .Property(x => x.LastContainerMasterUpdate)
            .Property(x => x.LastTerminalMasterUpdate)
            .Property(x => x.LocaleCode)
            .Property(x => x.OverrideFlag)
            .Property(x => x.Mdtid)
            .Property(x => x.TermId)
            .Property(x => x.RegionId)
            .Property(x => x.AreaId)
            .Property(x => x.PndVer)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.DriverStatus)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
