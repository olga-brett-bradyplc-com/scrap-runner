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

    public class CommodityMasterDestMap : ClassMapping<CommodityMasterDest>
    {
        public CommodityMasterDestMap()
        {
            Table("CommodityMasterDest");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(CommodityCode, ';', DestTerminalId)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.CommodityCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.DestTerminalId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.DestCustHostCode);
            Property(x => x.DestContainerLocation);
        }
    }
}
