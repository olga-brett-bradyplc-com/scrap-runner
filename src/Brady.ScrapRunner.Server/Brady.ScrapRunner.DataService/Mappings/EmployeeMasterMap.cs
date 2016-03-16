using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Mappings
{
    /// <summary>
    /// An EmployeeMaster mapping to NHibernate.  FIXME:  Remove the Id workaround for NHibernate 
    /// </summary>
    public class EmployeeMasterMap : ClassMapping<EmployeeMaster>
    {
        public EmployeeMasterMap()
        {

            Table("EmployeeMaster");

            Id(x => x.EmployeeId, m =>
            {
                m.Generator(Generators.Assigned);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("EmployeeId");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TerminalId);
            Property(x => x.FirstName);
            Property(x => x.LastName);
            Property(x => x.Address);
            Property(x => x.City);
            Property(x => x.State);
            Property(x => x.Zip);
            Property(x => x.Phone1);
            Property(x => x.Phone2);
            Property(x => x.Password);
            Property(x => x.FileMaintAccess);
            Property(x => x.SecurityLevel);
            Property(x => x.SupervisorId);
            Property(x => x.NickName);
            Property(x => x.EmployeeType);
            Property(x => x.CompanyCode);
            Property(x => x.EmployeeStatus);
            Property(x => x.LoginDateTime);
            Property(x => x.AccessDateTime);
            Property(x => x.WorkArea);
            Property(x => x.BillerInitials);
            Property(x => x.NumTimesLogin);
            Property(x => x.MaxLogins);
            Property(x => x.RouterId);
            Property(x => x.AreaId);
            Property(x => x.RegionId);
            Property(x => x.DefTerminalId);
            Property(x => x.PrevEmployeeId);
            Property(x => x.AllowMessaging);
            Property(x => x.DefLabelPrinter);
            Property(x => x.Country);
            Property(x => x.AllowMapsAccess);
            Property(x => x.DefReadyDateTomorrow);
            Property(x => x.AudibleAlertNewMsg);
            Property(x => x.MapId);
            Property(x => x.ActionFlag);
            Property(x => x.DefReadyDateMonday);
            Property(x => x.DefStartAcctSearchBegName);
            Property(x => x.AllowChangeContNumber);
            Property(x => x.AllowModDoneTrips);
            Property(x => x.AllowCancelDoneTrips);
            Property(x => x.opt);
            Property(x => x.SessionID);
            Property(x => x.Router);
            Property(x => x.DisplayReceiptNumber);
            Property(x => x.DisplayScaleReferenceNumber);
            Property(x => x.LoginID);
            Property(x => x.LoginIDPrev);
            Property(x => x.InactiveDate);
        }
    }
}
