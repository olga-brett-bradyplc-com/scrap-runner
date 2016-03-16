using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
 
    public class TripSegmentContainerTimeMetadata : TypeMetadataProvider<TripSegmentContainerTime>
    {
        public TripSegmentContainerTimeMetadata()
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

            IntegerProperty(x => x.TripSegContainerSeqNumber)
                .IsId()
                .DisplayName("Trip Seg Container SeqNumber");

            IntegerProperty(x => x.SeqNumber)
                .IsId()
                .DisplayName("Seq Number");

            StringProperty(x => x.TimeType);
            StringProperty(x => x.TimeDesc);
            IntegerProperty(x => x.ContainerTime);

            ViewDefaults()
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.TripSegContainerSeqNumber)
                .Property(x => x.SeqNumber)
                .Property(x => x.TimeType)
                .Property(x => x.TimeDesc)
                .Property(x => x.ContainerTime)

                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.TripSegNumber)
                .OrderBy(x => x.TripSegContainerSeqNumber)
                .OrderBy(x => x.SeqNumber);

        }
    }
}
