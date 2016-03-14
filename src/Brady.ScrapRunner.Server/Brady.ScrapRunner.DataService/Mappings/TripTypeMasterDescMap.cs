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

    public class TripTypeMasterDescMap : ClassMapping<TripTypeMasterDesc>
    {
        public TripTypeMasterDescMap()
        {
            Table("TripTypeMasterDesc");

            ComposedId(map =>
            {
                map.Property(y => y.TripTypeCode, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("TripTypeCode");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripTypeDesc);
            Property(x => x.TripTypePurchase);
            Property(x => x.TripTypeSale);
            Property(x => x.TripTypeScaleMsg);
            Property(x => x.TripTypeCompTripMsg);
        }
    }
}
