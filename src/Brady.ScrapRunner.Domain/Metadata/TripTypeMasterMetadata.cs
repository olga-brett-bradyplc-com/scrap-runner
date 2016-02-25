using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
 
    public class TripTypeMasterMetadata : TypeMetadataProvider<TripTypeMaster>
    {
        public TripTypeMasterMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripTypeCode)
                .IsId()
                .DisplayName("Trip Type Code");

            IntegerProperty(x => x.TripTypeSeqNumber)
                .IsId()
                .DisplayName("Trip Type Seq Number");

            StringProperty(x => x.TripTypeCodeBasic);
            StringProperty(x => x.CopyContainerId);
            StringProperty(x => x.CopyContainerType);
            StringProperty(x => x.CopyContainerSize);
            StringProperty(x => x.CopyCustomerLocation);
            StringProperty(x => x.CopyCommodityType);
            StringProperty(x => x.CopyCommoditySaleCustomer);
            StringProperty(x => x.UseCommodityTime);
            StringProperty(x => x.UseLocationTime);

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.TripTypeSeqNumber)
                .Property(x => x.TripTypeCodeBasic)
                .Property(x => x.CopyContainerId)
                .Property(x => x.CopyContainerType)
                .Property(x => x.CopyContainerSize)
                .Property(x => x.CopyCustomerLocation)
                .Property(x => x.CopyCommodityType)
                .Property(x => x.CopyCommoditySaleCustomer)
                .Property(x => x.UseCommodityTime)
                .Property(x => x.UseLocationTime)

                .OrderBy(x => x.TripTypeCode)
                .OrderBy(x => x.TripTypeSeqNumber);
        }
    }
}
