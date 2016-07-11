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
    /// A (trivial) DriverLogoffProcess mapping to NHibernate.
    /// </summary>
    public class DriverLogoffProcessMap : ClassMapping<DriverLogoffProcess>
    {
        public DriverLogoffProcessMap()
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

            Id(x => x.EmployeeId, m =>
            {
                m.Generator(Generators.Assigned);
            });

            //Property(x => x.TripSegNumber);
            //Property(x => x.DriverStatus);
            //Property(x => x.Odometer);
            //Property(x => x.ActionDateTime);
        }
    }
}