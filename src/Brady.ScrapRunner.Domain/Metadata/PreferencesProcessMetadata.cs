using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class PreferencesProcessMetadata : TypeMetadataProvider<PreferencesProcess>
    {
        public PreferencesProcessMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .OrderBy(x => x.EmployeeId);
        }
    }
}
