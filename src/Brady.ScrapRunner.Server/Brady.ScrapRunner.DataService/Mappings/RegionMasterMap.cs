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
    public class RegionMasterMap : ClassMapping<RegionMaster>
    {
        public RegionMasterMap()
        {
            Table("RegionMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("RegionId");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.RegionId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.RegionName);
            Property(x => x.Address1);
            Property(x => x.Address2);
            Property(x => x.City);
            Property(x => x.State);
            Property(x => x.Zip);
            Property(x => x.Country);
            Property(x => x.Phone);
        }
    }
}