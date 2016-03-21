using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cfg.MappingSchema;

namespace Brady.ScrapRunner.DataService.Mappings
{
    public class EventLogMap : ClassMapping<EventLog>
    {
        public EventLogMap()
        {
            Table("EventLog");

            // Mapping suggested by
            // http://stackoverflow.com/questions/20925197/using-nhibernate-to-insert-a-new-object-to-sql-server-gives-error-about-identity
            // http://stackoverflow.com/questions/7279473/using-nhibernate-mapping-by-code-cannot-insert-explicit-value-for-identity-colu

            Id(x => x.EventId, m =>
            {
                m.UnsavedValue(0);
                m.Generator(Generators.Identity);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("EventId");
                m.Insert(false);
                m.Update(false);
                m.Generated(PropertyGeneration.Never);
            });

            Property(x => x.EventDateTime);
            Property(x => x.EventSeqNo);
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