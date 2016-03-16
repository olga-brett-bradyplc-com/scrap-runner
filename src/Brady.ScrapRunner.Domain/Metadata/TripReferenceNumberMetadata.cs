using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;


namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TripReferenceNumberMetadata : TypeMetadataProvider<TripReferenceNumber>
    {
        public TripReferenceNumberMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            IntegerProperty(x => x.TripSeqNumber)
                .IsId()
                .DisplayName("Trip Seq Number");

            StringProperty(x => x.TripRefNumberDesc);
            StringProperty(x => x.TripRefNumber);

            ViewDefaults()
                .Property(x => x.TripNumber)
                .Property(x => x.TripSeqNumber)
                .Property(x => x.TripRefNumberDesc)
                .Property(x => x.TripRefNumber)

                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.TripSeqNumber);
        }
    }
}
