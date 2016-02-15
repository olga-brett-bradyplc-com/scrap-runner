using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class AreaMasterMetadata : TypeMetadataProvider<AreaMaster>
    {
        public AreaMasterMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.AreaId)
                .IsId()
                .DisplayName("Area Id");

            StringProperty(x => x.TerminalId)
                .IsId()
                .DisplayName("Terminal Id");

            ViewDefaults()
                .Property(x => x.AreaId)
                .Property(x => x.TerminalId)
                .OrderBy(x => x.AreaId)
                .OrderBy(x => x.TerminalId);
        }
    }
}
