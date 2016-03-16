using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CustomerCommodityMetadata : TypeMetadataProvider<CustomerCommodity>
    {
        public CustomerCommodityMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CustHostCode)
                .IsId()
                .DisplayName("Cust Host Code");

            StringProperty(x => x.CustCommodityCode)
                .IsId()
                .DisplayName("Cust Commodity Code");

            StringProperty(x => x.MasterCommodityCode);
            StringProperty(x => x.CustCommodityDesc);
            StringProperty(x => x.CustContainerType);
            StringProperty(x => x.CustContainerSize);
            StringProperty(x => x.CustContainerLocation);
            StringProperty(x => x.DestCustHostCode);
            StringProperty(x => x.DestContainerLocation);
            DateProperty(x => x.DestExpirationDate);
            IntegerProperty(x => x.CustStandardMinutes);

            ViewDefaults()
                .Property(x => x.CustHostCode)
                .Property(x => x.CustCommodityCode)
                .Property(x => x.MasterCommodityCode)
                .Property(x => x.CustCommodityDesc)
                .Property(x => x.CustContainerType)
                .Property(x => x.CustContainerSize)
                .Property(x => x.CustContainerLocation)
                .Property(x => x.DestCustHostCode)
                .Property(x => x.DestContainerLocation)
                .Property(x => x.DestExpirationDate)
                .Property(x => x.CustStandardMinutes)

                .OrderBy(x => x.CustHostCode)
                .OrderBy(x => x.CustCommodityCode);
        }
    }
}
