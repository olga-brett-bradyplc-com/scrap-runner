using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TripTypeBasicDetailsMetadata : TypeMetadataProvider<TripTypeBasicDetails>
    {
        public TripTypeBasicDetailsMetadata()
        {
            AutoUpdatesByDefault();

            IntegerProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            IntegerProperty(x => x.SeqNo)
                .IsId()
                .IsNotEditableInGrid()
                .DisplayName("Seq Number");

            StringProperty(x => x.TripTypeCode)
                .DisplayName("Trip Type Code");
            StringProperty(x => x.ContainerType)
                .DisplayName("Container Type");
            StringProperty(x => x.ContainerSize);
            IntegerProperty(x => x.FirstCTRTime);
            IntegerProperty(x => x.SecondCTRTime);

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.FirstCTRTime)
                .Property(x => x.SecondCTRTime)
                .Property(x => x.SeqNo)

                .OrderBy(x => x.TripTypeCode)
                .OrderBy(x => x.ContainerType)
                .OrderBy(x => x.SeqNo);
        }
    }
}
