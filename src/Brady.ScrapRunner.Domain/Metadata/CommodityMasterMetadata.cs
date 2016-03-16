using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CommodityMasterMetadata : TypeMetadataProvider<CommodityMaster>
    {
        public CommodityMasterMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CommodityCode)
                .IsId()
                .DisplayName("Commodity Code");

            StringProperty(x => x.CommodityDesc);
            StringProperty(x => x.InventoryCode);
            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            StringProperty(x => x.DestCustHostCode);
            StringProperty(x => x.DestContainerLocation);
            StringProperty(x => x.InactiveFlag);
            StringProperty(x => x.UniversalFlag);

            ViewDefaults()
                .Property(x => x.CommodityCode)
                .Property(x => x.CommodityDesc)
                .Property(x => x.InventoryCode)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.DestCustHostCode)
                .Property(x => x.DestContainerLocation)
                .Property(x => x.InactiveFlag)
                .Property(x => x.UniversalFlag)
                .OrderBy(x => x.CommodityCode);
        }
    }
}
