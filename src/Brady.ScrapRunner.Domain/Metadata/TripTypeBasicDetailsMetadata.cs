using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{

    public class TripTypeBasicDetailsMetadata : TypeMetadataProvider<TripTypeBasicDetails>
    {
        public TripTypeBasicDetailsMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripTypeCode)
                .IsId()
                .DisplayName("Trip Type Code");

            IntegerProperty(x => x.SeqNo)
                .IsId()
                .IsNotEditableInGrid()
                .DisplayName("Seq Number");

            StringProperty(x => x.ContainerType)
               .IsId()
               .DisplayName("Container Type");

            StringProperty(x => x.ContainerSize);
            IntegerProperty(x => x.FirstCTRTime);
            IntegerProperty(x => x.SecondCTRTime);

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.SeqNo)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.FirstCTRTime)
                .Property(x => x.SecondCTRTime)

                .OrderBy(x => x.TripTypeCode)
                .OrderBy(x => x.SeqNo)
                .OrderBy(x => x.ContainerType);
        }
    }
}
