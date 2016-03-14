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
 
    public class TripTypeBasicDetailsMap : ClassMapping<TripTypeBasicDetails>
    {
        public TripTypeBasicDetailsMap()
        {
            Table("TripTypeBasicDetails");

            ComposedId(map =>
            {
                map.Property(y => y.ContainerType, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.SeqNo, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripTypeCode, m => m.Generated(PropertyGeneration.Never));
            });

             Property(x => x.Id, m =>
            {
                m.Formula("concat(ContainerType, ';', SeqNo, ';', TripTypeCode)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ContainerSize);
            Property(x => x.FirstCTRTime);
            Property(x => x.SecondCTRTime);

        }
    }
}
