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
    public class TripTypeBasicMap : ClassMapping<TripTypeBasic>
    {
        public TripTypeBasicMap()
        {
            Table("TripTypeBasic");

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
            Property(x => x.TripTypeHostCode);
            Property(x => x.TripTypeHostCodeScale);
            Property(x => x.TripTypeStandardMinutes);
        }
    }
}