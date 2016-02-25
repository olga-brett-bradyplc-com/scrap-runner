using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
  
    public class CommodityMasterDestMetadata : TypeMetadataProvider<CommodityMasterDest>
    {
        public CommodityMasterDestMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CommodityCode)
                .IsId()
                .DisplayName("Commodity Code");

            StringProperty(x => x.DestTerminalId)
                .IsId()
                .DisplayName("Dest Terminal Id");

            StringProperty(x => x.DestCustHostCode);
            StringProperty(x => x.DestContainerLocation);

            ViewDefaults()
                .Property(x => x.CommodityCode)
                .Property(x => x.DestTerminalId)
                .Property(x => x.DestCustHostCode)
                .Property(x => x.DestContainerLocation)

                .OrderBy(x => x.CommodityCode)
                .OrderBy(x => x.DestTerminalId);
        }
    }
}
