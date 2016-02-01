using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class ErrorLogMetadata : TypeMetadataProvider<ErrorLog>
    {
        public ErrorLogMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.ErrorDateTime)
                .IsId()
                .DisplayName("Error DateTime");

            IntegerProperty(x => x.ErrorSeqNo)
                .IsId()
                .DisplayName("Error SeqNo");

            StringProperty(x => x.ErrorTerminalId);
            StringProperty(x => x.ErrorRegionId);
            StringProperty(x => x.ErrorType);
            StringProperty(x => x.ErrorDescription);
            StringProperty(x => x.ErrorTerminalId);
            StringProperty(x => x.ErrorRegionId);
            StringProperty(x => x.ErrorFlag);
            StringProperty(x => x.ErrorContainerNumber);
            StringProperty(x => x.ErrorTripNumber);

            ViewDefaults()
                .Property(x => x.ErrorDateTime)
                .Property(x => x.ErrorSeqNo)
                .Property(x => x.ErrorType)
                .Property(x => x.ErrorDescription)
                .Property(x => x.ErrorTerminalId)
                .Property(x => x.ErrorRegionId)
                .Property(x => x.ErrorFlag)
                .Property(x => x.ErrorContainerNumber)
                .Property(x => x.ErrorTripNumber)

                .OrderBy(x => x.ErrorDateTime)
                .OrderBy(x => x.ErrorSeqNo);
        }
    }
}
