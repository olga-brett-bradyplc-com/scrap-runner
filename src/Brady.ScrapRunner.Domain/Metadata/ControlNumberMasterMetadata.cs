using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
   
    public class ControlNumberMasterMetadata : TypeMetadataProvider<ControlNumberMaster>
    {
        public ControlNumberMasterMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.ControlType)
                .IsId()
                .DisplayName("Control Type");

            IntegerProperty(x => x.ControlNumberNext);

            ViewDefaults()
                .Property(x => x.ControlType)
                .Property(x => x.ControlNumberNext)

                .OrderBy(x => x.ControlType);
        }
    }
}
