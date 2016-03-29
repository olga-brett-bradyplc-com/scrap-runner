using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    class CodeTableProcessMetadata : TypeMetadataProvider<CodeTableProcess>
    {
        public CodeTableProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.CodeName);
            StringProperty(x => x.CodeValue);
            StringProperty(x => x.CodeDisp1);
            StringProperty(x => x.CodeDisp2);
            StringProperty(x => x.CodeDisp3);
            StringProperty(x => x.CodeDisp4);
            StringProperty(x => x.CodeDisp5);
            StringProperty(x => x.CodeDisp6);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.CodeName)
            .Property(x => x.CodeValue)
            .Property(x => x.CodeDisp1)
            .Property(x => x.CodeDisp2)
            .Property(x => x.CodeDisp3)
            .Property(x => x.CodeDisp4)
            .Property(x => x.CodeDisp5)
            .Property(x => x.CodeDisp6)
            .OrderBy(x => x.EmployeeId);
        }
    }
}
