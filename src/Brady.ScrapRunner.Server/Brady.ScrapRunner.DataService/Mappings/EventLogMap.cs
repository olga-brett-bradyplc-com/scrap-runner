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
    public class EventLogMap : ClassMapping<EventLog>
    {
        public EventLogMap()
        {
            Table("EventLog");

            Property(x => x.Id, m =>
            {
                m.Formula("CONCAT(CONVERT(VARCHAR(33), EventDateTime, 126), ';', EventSeqNo)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.EventDateTime, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.EventSeqNo, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.EventTerminalId);
            Property(x => x.EventRegionId);
            Property(x => x.EventEmployeeId);
            Property(x => x.EventEmployeeName);
            Property(x => x.EventTripNumber);
            Property(x => x.EventProgram);
            Property(x => x.EventScreen);
            Property(x => x.EventAction);
            Property(x => x.EventComment);
        }
    }
}