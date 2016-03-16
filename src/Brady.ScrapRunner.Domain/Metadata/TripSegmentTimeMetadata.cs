using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;


namespace Brady.ScrapRunner.Domain.Metadata
{
 
    public class TripSegmentTimeMetadata : TypeMetadataProvider<TripSegmentTime>
    {
        public TripSegmentTimeMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber)
                .IsId()
                .DisplayName("Trip Seg Number");

            IntegerProperty(x => x.SeqNumber)
                .IsId()
                .DisplayName("Seq Number");

            StringProperty(x => x.TimeType);
            StringProperty(x => x.TimeDesc);
            IntegerProperty(x => x.SegmentTime);

            ViewDefaults()
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.SeqNumber)
                .Property(x => x.TimeType)
                .Property(x => x.TimeDesc)
                .Property(x => x.SegmentTime)

                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.TripSegNumber)
                .OrderBy(x => x.SeqNumber);

        }
    }
}
