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
 
    public class TripSegmentTimeMap : ClassMapping<TripSegmentTime>
    {
        public TripSegmentTimeMap()
        {

            Table("TripSegmentTime");

            ComposedId(map =>
            {
                map.Property(y => y.SeqNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(SeqNumber, ';', TripNumber, ';', TripSegNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TimeType);
            Property(x => x.TimeDesc);
            Property(x => x.SegmentTime);
         }
    }
}
