using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using BWF.DataServices.Metadata.Fluent.Enums;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class ErrorLogMetadata : TypeMetadataProvider<ErrorLog>
    {
        public ErrorLogMetadata()
        {

            AutoUpdatesByDefault();

            IntegerProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            IntegerProperty(x => x.ErrorId)
                .IsId()
                .IsNotEditableInGrid()
                .DisplayName("Error Id");

            TimeProperty(x => x.ErrorDateTime);
            IntegerProperty(x => x.ErrorSeqNo);
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
                .Property(x => x.ErrorId)
                .Property(x => x.ErrorDateTime)
                .Property(x => x.ErrorSeqNo)
                .Property(x => x.ErrorType)
                .Property(x => x.ErrorDescription)
                .Property(x => x.ErrorTerminalId)
                .Property(x => x.ErrorRegionId)
                .Property(x => x.ErrorFlag)
                .Property(x => x.ErrorContainerNumber)
                .Property(x => x.ErrorTripNumber)

                .OrderBy(x => x.ErrorId, Direction.Descending);
        }
    }
}
