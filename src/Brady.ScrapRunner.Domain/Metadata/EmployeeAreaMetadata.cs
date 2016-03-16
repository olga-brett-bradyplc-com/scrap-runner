using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class EmployeeAreaMetadata : TypeMetadataProvider<EmployeeArea>
    {
        public EmployeeAreaMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.AreaId)
               .IsId()
               .DisplayName("Area Id");

            IntegerProperty(x => x.ButtonNumber);
            StringProperty(x => x.DefaultTerminalId);
            IntegerProperty(x => x.KeyCode);
            StringProperty(x => x.KeyDescription);

            ViewDefaults()
                .Property(x => x.EmployeeId)
                .Property(x => x.AreaId)
                .Property(x => x.ButtonNumber)
                .Property(x => x.DefaultTerminalId)
                .Property(x => x.KeyCode)
                .Property(x => x.KeyDescription)

                .OrderBy(x => x.EmployeeId)
                .OrderBy(x => x.AreaId);

        }
    }
}
