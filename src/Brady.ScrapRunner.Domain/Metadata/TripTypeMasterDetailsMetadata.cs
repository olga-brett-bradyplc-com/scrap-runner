using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TripTypeMasterDetailsMetadata : TypeMetadataProvider<TripTypeMasterDetails>
    {
        public TripTypeMasterDetailsMetadata()
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

            StringProperty(x => x.AccessorialCode)
                .IsId()
                .DisplayName("Accessorial Code");

            IntegerProperty(x => x.NumberOfContainers);
            StringProperty(x => x.ActivationFlag);

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.TripTypeSeqNumber)
                .Property(x => x.AccessorialCode)
                .Property(x => x.NumberOfContainers)
                .Property(x => x.ActivationFlag)

                .OrderBy(x => x.TripTypeCode)
                .OrderBy(x => x.TripTypeSeqNumber)
                .OrderBy(x => x.AccessorialCode);
        }
    }
}
