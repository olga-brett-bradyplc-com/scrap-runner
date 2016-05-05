using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Mappings
{
    /// <summary>
    /// DriverContainerActionProcess mapping to NHibernate.
    /// </summary>
    public class DriverSegmentDoneProcessMap : ClassMapping<DriverSegmentDoneProcess>
    {
        public DriverSegmentDoneProcessMap()
        {
            Table("EmployeeMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("EmployeeId");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.EmployeeId, m => m.Generated(PropertyGeneration.Never));
            });

            //Property(x => x.TripNumber);
            //Property(x => x.TripSegNumber);
        }
    }
}
