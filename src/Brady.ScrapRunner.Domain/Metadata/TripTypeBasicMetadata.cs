using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TripTypeBasicMetadata : TypeMetadataProvider<TripTypeBasic>
    {
        public TripTypeBasicMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripTypeCode)
                .IsId()
                .DisplayName("Trip TypeCode");

            StringProperty(x => x.TripTypeDesc);
            StringProperty(x => x.TripTypeHostCode);
            StringProperty(x => x.TripTypeHostCodeScale);
            IntegerProperty(x => x.TripTypeStandardMinutes);

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.TripTypeDesc)
                .Property(x => x.TripTypeHostCode)
                .Property(x => x.TripTypeHostCodeScale)
                .Property(x => x.TripTypeStandardMinutes)
                .OrderBy(x => x.TripTypeCode);
        }
    }
}
