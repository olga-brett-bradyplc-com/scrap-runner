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
 
    public class PowerLimitsMap : ClassMapping<PowerLimits>
    {
        public PowerLimitsMap()
        {

            Table("PowerLimits");

            ComposedId(map =>
            {
                map.Property(y => y.PowerId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.PowerSeqNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.ContainerType, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(ContainerType, ';', PowerId, ';', PowerSeqNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ContainerMinSize);
            Property(x => x.ContainerMaxSize);
         }
    }
}
