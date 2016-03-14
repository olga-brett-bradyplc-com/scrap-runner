using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class ContainerChangeMetadata : TypeMetadataProvider<ContainerChange>
    {
        public ContainerChangeMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.ContainerNumber)
                .IsId()
                .DisplayName("Container Number");

            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            TimeProperty(x => x.ActionDate);
            StringProperty(x => x.ActionFlag);
            StringProperty(x => x.TerminalId);
            StringProperty(x => x.RegionId);
            StringProperty(x => x.ContainerBarCodeNo);

            ViewDefaults()
                .Property(x => x.ContainerNumber)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.ActionDate)
                .Property(x => x.ActionFlag)
                .Property(x => x.TerminalId)
                .Property(x => x.RegionId)
                .Property(x => x.ContainerBarCodeNo)

                .OrderBy(x => x.ContainerNumber);
        }
    }
}
