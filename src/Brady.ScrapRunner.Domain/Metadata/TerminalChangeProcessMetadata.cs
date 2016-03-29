using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TerminalChangeProcessMetadata: TypeMetadataProvider<TerminalChangeProcess>
    {
       public TerminalChangeProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            TimeProperty(x => x.LastTerminalChangeUpdate);
            StringProperty(x => x.TerminalId);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.LastTerminalChangeUpdate)
            .OrderBy(x => x.EmployeeId);
        }
    }
}
