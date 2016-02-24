using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
 
    public class DriverMasterMetadata : TypeMetadataProvider<DriverMaster>
    {
        public DriverMasterMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            DateProperty(x => x.StartingTime);
            DateProperty(x => x.QuitTime);
            DateProperty(x => x.HoursPerDay);
            StringProperty(x => x.DispatcherId);
            StringProperty(x => x.Route);
            StringProperty(x => x.TerminalId);
            StringProperty(x => x.Tractor);
            StringProperty(x => x.DLicenseNumber);
            StringProperty(x => x.DLicenseState);
            DateProperty(x => x.DLicenseExpiry);
            StringProperty(x => x.OwnerOperFlag);
            DateProperty(x => x.SeniorityDate);
            DateProperty(x => x.NextPhysDate);
            DateProperty(x => x.BegVacDate);
            DateProperty(x => x.EndVacDate);
            StringProperty(x => x.Comments);
            StringProperty(x => x.TrailerQualified);
            StringProperty(x => x.PowerQualified);
            StringProperty(x => x.ContractDriverFlag);
 

            ViewDefaults()
                .Property(x => x.EmployeeId)
                .Property(x => x.StartingTime)
                .Property(x => x.QuitTime)
                .Property(x => x.HoursPerDay)
                .Property(x => x.DispatcherId)
                .Property(x => x.Route)
                .Property(x => x.TerminalId)
                .Property(x => x.Tractor)
                .Property(x => x.DLicenseNumber)
                .Property(x => x.DLicenseState)
                .Property(x => x.DLicenseExpiry)
                .Property(x => x.OwnerOperFlag)
                .Property(x => x.SeniorityDate)
                .Property(x => x.NextPhysDate)
                .Property(x => x.BegVacDate)
                .Property(x => x.EndVacDate)
                .Property(x => x.Comments)
                .Property(x => x.TrailerQualified)
                .Property(x => x.PowerQualified)
                .Property(x => x.ContractDriverFlag)

                .OrderBy(x => x.EmployeeId);

        }
    }
}
