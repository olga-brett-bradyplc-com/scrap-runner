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
    /// An AreaMaster mapping to NHibernate.
    /// </summary>
    public class AreaMasterMap : ClassMapping<AreaMaster>
    {
        public AreaMasterMap()
        {
            Table("AreaMaster");

            ComposedId(map =>
            {
                map.Property(y => y.AreaId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TerminalId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(AreaId, ';', TerminalId)");
                m.Insert(false);
                m.Update(false);
            });
        }
    }
}
