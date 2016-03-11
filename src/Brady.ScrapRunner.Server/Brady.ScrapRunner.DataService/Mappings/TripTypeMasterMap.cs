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
   
    public class TripTypeMasterMap : ClassMapping<TripTypeMaster>
    {
        public TripTypeMasterMap()
        {
            Table("TripTypeMaster");

            ComposedId(map =>
            {
                map.Property(y => y.TripTypeCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripTypeSeqNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripTypeCode, ';', TripTypeSeqNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripTypeCodeBasic);
            Property(x => x.CopyContainerId);
            Property(x => x.CopyContainerType);
            Property(x => x.CopyContainerSize);
            Property(x => x.CopyCustomerLocation);
            Property(x => x.CopyCommodityType);
            Property(x => x.CopyCommoditySaleCustomer);
            Property(x => x.UseCommodityTime);
            Property(x => x.UseLocationTime);
        }
    }
}
