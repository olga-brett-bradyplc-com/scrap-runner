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

    public class TripSegmentImageMap : ClassMapping<TripSegmentImage>
    {
        public TripSegmentImageMap()
        {

            Table("TripSegmentImage");

            ComposedId(map =>
            {
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegImageSeqId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripNumber, ';', TripSegImageSeqId, ';', TripSegNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripSegImageActionDateTime);
            Property(x => x.TripSegImageLocation);
            Property(x => x.TripSegImagePrintedName);
            Property(x => x.TripSegImageType);
        }
    }
}
