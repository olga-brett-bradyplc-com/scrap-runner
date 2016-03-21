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
    /// A DriverMaster mapping to NHibernate.   
    /// </summary>

    public class DriverMasterMap : ClassMapping<DriverMaster>
    {
        public DriverMasterMap()
        {

            Table("DriverMaster");

            ComposedId(map =>
            {
                map.Property(y => y.EmployeeId,m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("EmployeeId");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.StartingTime);
            Property(x => x.QuitTime);
            Property(x => x.HoursPerDay);
            Property(x => x.DispatcherId);
            Property(x => x.Route);
            Property(x => x.TerminalId);
            Property(x => x.Tractor);
            Property(x => x.DLicenseNumber);
            Property(x => x.DLicenseState);
            Property(x => x.DLicenseExpiry);
            Property(x => x.OwnerOperFlag);
            Property(x => x.SeniorityDate);
            Property(x => x.NextPhysDate);
            Property(x => x.BegVacDate);
            Property(x => x.EndVacDate);
            Property(x => x.Comments);
            Property(x => x.TrailerQualified);
            Property(x => x.PowerQualified);
            Property(x => x.ContractDriverFlag);
        }
    }
}
