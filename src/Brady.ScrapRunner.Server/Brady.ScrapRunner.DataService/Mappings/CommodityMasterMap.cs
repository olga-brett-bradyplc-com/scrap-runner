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
    public class CommodityMasterMap : ClassMapping<CommodityMaster>
    {
        public CommodityMasterMap()
        {
            Table("CommodityMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("CommodityCode");
                m.Insert(false);
                m.Update(false);
            });
      
            ComposedId(map =>
            {
                map.Property(y => y.CommodityCode, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.CommodityDesc);
            Property(x => x.InventoryCode);
            Property(x => x.ContainerType);
            Property(x => x.ContainerSize);
            Property(x => x.DestCustHostCode);
            Property(x => x.DestContainerLocation);
            Property(x => x.InactiveFlag);
            Property(x => x.UniversalFlag);
        }
    }
}