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

    public class TripTypeAllowCustTypeMap : ClassMapping<TripTypeAllowCustType>
    {
        public TripTypeAllowCustTypeMap()
        {
            Table("TripTypeAllowCustType");

            ComposedId(map =>
            {
                map.Property(y => y.TripTypeCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripTypeCustType, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripTypeSeqNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripTypeCode, ';', TripTypeCustType, ';', TripTypeSeqNumber)");
                m.Insert(false);
                m.Update(false);
            });

        }
    }
}
