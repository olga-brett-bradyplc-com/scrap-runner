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
    public class PowerFuelMap : ClassMapping<PowerFuel>
    {
        public PowerFuelMap()
        {

            Table("PowerFuel");

            ComposedId(map =>
            {
                map.Property(y => y.PowerId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.PowerFuelSeqNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(PowerFuelSeqNumber, ';', PowerId, ';', TripNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripSegNumber);
            Property(x => x.TripTerminalId);
            Property(x => x.TripRegionId);
            Property(x => x.TripDriverId);
            Property(x => x.TripDriverName);
            Property(x => x.PowerDateOfFuel);
            Property(x => x.PowerState);
            Property(x => x.PowerCountry);
            Property(x => x.PowerOdometer);
            Property(x => x.PowerGallons);

        }
    }
}
