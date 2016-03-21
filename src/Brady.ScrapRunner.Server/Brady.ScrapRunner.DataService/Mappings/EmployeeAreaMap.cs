using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Brady.ScrapRunner.DataService.Mappings
{
    /// <summary>
    /// An EmployeeArea mapping to NHibernate.
    /// </summary>
    public class EmployeeAreaMap : ClassMapping<EmployeeArea>
    {
        public EmployeeAreaMap()
        {
            Table("EmployeeArea");

            ComposedId(map =>
            {
                map.Property(y => y.EmployeeId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.AreaId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(AreaId, ';', EmployeeId)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ButtonNumber);
            Property(x => x.DefaultTerminalId);
            Property(x => x.KeyCode);
            Property(x => x.KeyDescription);
        }
    }
}
