using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class EventLogMetadata : TypeMetadataProvider<EventLog>
    {
        public EventLogMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EventDateTime)
                .IsId()
                .DisplayName("Event DateTime");

            IntegerProperty(x => x.EventSeqNo)
                .IsId()
                .DisplayName("Event SeqNo");

            StringProperty(x => x.EventTerminalId);
            StringProperty(x => x.EventRegionId);
            StringProperty(x => x.EventEmployeeId);
            StringProperty(x => x.EventEmployeeName);
            StringProperty(x => x.EventTripNumber);
            StringProperty(x => x.EventProgram);
            StringProperty(x => x.EventScreen);
            StringProperty(x => x.EventAction);
            StringProperty(x => x.EventComment);

            ViewDefaults()
                .Property(x => x.EventDateTime)
                .Property(x => x.EventSeqNo)
                .Property(x => x.EventTerminalId)
                .Property(x => x.EventRegionId)
                .Property(x => x.EventEmployeeId)
                .Property(x => x.EventEmployeeName)
                .Property(x => x.EventTripNumber)
                .Property(x => x.EventProgram)
                .Property(x => x.EventScreen)
                .Property(x => x.EventAction)
                .Property(x => x.EventComment)

                .OrderBy(x => x.EventDateTime)
                .OrderBy(x => x.EventSeqNo);
        }
    }
}
