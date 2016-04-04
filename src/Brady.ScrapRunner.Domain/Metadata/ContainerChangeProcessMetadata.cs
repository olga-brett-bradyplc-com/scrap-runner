using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class ContainerChangeProcessMetadata : TypeMetadataProvider<ContainerChangeProcess>
    {
        public ContainerChangeProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            TimeProperty(x => x.LastContainerMasterUpdate);
            StringProperty(x => x.ContainerNumber);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.LastContainerMasterUpdate)
            .Property(x => x.ContainerNumber)
            .OrderBy(x => x.EmployeeId);
        }
    }
}
