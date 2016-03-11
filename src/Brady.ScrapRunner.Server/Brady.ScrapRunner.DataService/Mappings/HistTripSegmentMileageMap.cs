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
    public class HistTripSegmentMileageMap : ClassMapping<HistTripSegmentMileage>
    {
        public HistTripSegmentMileageMap()
        {

            Table("HistTripSegmentMileage");

            ComposedId(map =>
            {
                map.Property(y => y.HistSeqNo, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegMileageSeqNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(HistSeqNo, ';', TripNumber, ';', TripSegMileageSeqNumber, ';', TripSegNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripSegMileageState);
            Property(x => x.TripSegMileageCountry);
            Property(x => x.TripSegMileageOdometerStart);
            Property(x => x.TripSegMileageOdometerEnd);
            Property(x => x.TripSegLoadedFlag);
            Property(x => x.TripSegMileagePowerId);
            Property(x => x.TripSegMileageDriverId);
            Property(x => x.TripSegMileageDriverName);
        }
    }
}
