using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class RegionMasterMetadata : TypeMetadataProvider<RegionMaster>
    {
        public RegionMasterMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.RegionId)
                .IsId()
                .DisplayName("Region Id");

            StringProperty(x => x.RegionName);
            StringProperty(x => x.Address1);
            StringProperty(x => x.Address2);
            StringProperty(x => x.City);
            StringProperty(x => x.State);
            StringProperty(x => x.Zip);
            StringProperty(x => x.Country);
            StringProperty(x => x.Phone);

            ViewDefaults()
                .Property(x => x.RegionId)
                .Property(x => x.RegionName)
                .Property(x => x.Address1)
                .Property(x => x.Address2)
                .Property(x => x.City)
                .Property(x => x.State)
                .Property(x => x.Zip)
                .Property(x => x.Country)
                .Property(x => x.Phone)
                .OrderBy(x => x.RegionId);
        }
    }
}
