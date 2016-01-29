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
    public class CustomerCommodityMap : ClassMapping<CustomerCommodity>
    {
        public CustomerCommodityMap()
        {
            Table("CustomerCommodity");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(CustHostCode, ';', CustCommodityCode)");
                m.Insert(false);
                m.Update(false);
            });
      
            ComposedId(map =>
            {
                map.Property(y => y.CustHostCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.CustCommodityCode, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.MasterCommodityCode);
            Property(x => x.CustCommodityDesc);
            Property(x => x.CustContainerType);
            Property(x => x.CustContainerSize);
            Property(x => x.CustContainerLocation);
            Property(x => x.DestCustHostCode);
            Property(x => x.DestContainerLocation);
            Property(x => x.DestExpirationDate);
            Property(x => x.CustStandardMinutes);
        }
    }
}