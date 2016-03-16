using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    
    public class TripTypeMasterDescMetadata : TypeMetadataProvider<TripTypeMasterDesc>
    {
        public TripTypeMasterDescMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripTypeCode)
                .IsId()
                .DisplayName("Trip TypeCode");

            StringProperty(x => x.TripTypeDesc);
            StringProperty(x => x.TripTypePurchase);
            StringProperty(x => x.TripTypeSale);
            StringProperty(x => x.TripTypeScaleMsg);
            StringProperty(x => x.TripTypeCompTripMsg);

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.TripTypeDesc)
                .Property(x => x.TripTypePurchase)
                .Property(x => x.TripTypeSale)
                .Property(x => x.TripTypeScaleMsg)
                .Property(x => x.TripTypeCompTripMsg)

                .OrderBy(x => x.TripTypeCode);
        }
    }
}
