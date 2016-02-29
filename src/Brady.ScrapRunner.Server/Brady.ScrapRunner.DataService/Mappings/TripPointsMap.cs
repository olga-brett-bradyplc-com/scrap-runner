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

    public class TripPointsMap : ClassMapping<TripPoints>
    {
        public TripPointsMap()
        {

            Table("TripPoints");

            ComposedId(map =>
            {
                map.Property(y => y.TripPointsHostCode1, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripPointsHostCode2, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripPointsHostCode1, ';', TripPointsHostCode2)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripPointsStandardMinutes);
            Property(x => x.TripPointsStandardMiles);
            Property(x => x.TripPointsSendToMaps);
            Property(x => x.ChgDateTime);
            Property(x => x.ChgEmployeeId);
        }
    }
}
