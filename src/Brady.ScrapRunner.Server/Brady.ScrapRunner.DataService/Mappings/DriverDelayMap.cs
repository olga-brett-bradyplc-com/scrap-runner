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
    public class DriverDelayMap : ClassMapping<DriverDelay>
    {
        public DriverDelayMap()
        {
            Table("DriverDelay");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(DelaySeqNumber, ';', DriverId, ';', TripNumber)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.DelaySeqNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.DriverId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.TripSegNumber);
            Property(x => x.DriverName);
            Property(x => x.DelayCode);
            Property(x => x.DelayReason);
            Property(x => x.DelayStartDateTime);
            Property(x => x.DelayEndDateTime);
            Property(x => x.DelayLatitude);
            Property(x => x.DelayLongitude);
            Property(x => x.TerminalId);
            Property(x => x.RegionId);
            Property(x => x.TerminalName);
            Property(x => x.RegionName);
        }
    }
}