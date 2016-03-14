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
    public class CustomerLocationMap : ClassMapping<CustomerLocation>
    {
        public CustomerLocationMap()
        {
            Table("CustomerLocation");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(CustHostCode, ';', CustLocation)");
                m.Insert(false);
                m.Update(false);
            });
      
            ComposedId(map =>
            {
                map.Property(y => y.CustHostCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.CustLocation, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.CustStandardMinutes);
        }
    }
}