using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class EmployeeMasterMetadata : TypeMetadataProvider<EmployeeMaster>
    {
        public EmployeeMasterMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.TerminalId);
            StringProperty(x => x.FirstName);
            StringProperty(x => x.LastName);
            StringProperty(x => x.Address);
            StringProperty(x => x.City);
            StringProperty(x => x.State);
            StringProperty(x => x.Zip);
            StringProperty(x => x.Phone1);
            StringProperty(x => x.Phone2);
            StringProperty(x => x.Password);
            StringProperty(x => x.FileMaintAccess);
            StringProperty(x => x.SecurityLevel);
            StringProperty(x => x.SupervisorId);
            StringProperty(x => x.NickName);
            StringProperty(x => x.EmployeeType);
            StringProperty(x => x.CompanyCode);
            StringProperty(x => x.EmployeeStatus);
            DateProperty(x => x.LoginDateTime);
            DateProperty(x => x.AccessDateTime);
            StringProperty(x => x.WorkArea);
            StringProperty(x => x.BillerInitials);
            IntegerProperty(x => x.NumTimesLogin);
            IntegerProperty(x => x.MaxLogins);
            StringProperty(x => x.RouterId);
            StringProperty(x => x.AreaId);
            StringProperty(x => x.RegionId);
            StringProperty(x => x.DefTerminalId);
            StringProperty(x => x.PrevEmployeeId);
            StringProperty(x => x.AllowMessaging);
            StringProperty(x => x.DefLabelPrinter);
            StringProperty(x => x.Country);
            StringProperty(x => x.AllowMapsAccess);
            StringProperty(x => x.DefReadyDateTomorrow);
            StringProperty(x => x.AudibleAlertNewMsg);
            StringProperty(x => x.MapId);
            StringProperty(x => x.ActionFlag);
            StringProperty(x => x.DefReadyDateMonday);
            StringProperty(x => x.DefStartAcctSearchBegName);
            StringProperty(x => x.AllowChangeContNumber);
            StringProperty(x => x.AllowModDoneTrips);
            StringProperty(x => x.AllowCancelDoneTrips);
            StringProperty(x => x.opt);
            IntegerProperty(x => x.SessionID);
            StringProperty(x => x.Router);
            StringProperty(x => x.DisplayReceiptNumber);
            StringProperty(x => x.DisplayScaleReferenceNumber);
            StringProperty(x => x.LoginID);
            StringProperty(x => x.LoginIDPrev);
            DateProperty(x => x.InactiveDate);

            ViewDefaults()
                .Property(x => x.EmployeeId)
                .Property(x => x.TerminalId)
                .Property(x => x.FirstName)
                .Property(x => x.LastName)
                .Property(x => x.Address)
                .Property(x => x.City)
                .Property(x => x.State)
                .Property(x => x.Zip)
                .Property(x => x.Phone1)
                .Property(x => x.Phone2)
                .OrderBy(x => x.EmployeeId);

        }
    }
}
