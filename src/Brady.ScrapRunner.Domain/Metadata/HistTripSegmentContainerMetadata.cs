using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class HistTripSegmentContainerMetadata : TypeMetadataProvider<HistTripSegmentContainer>
    {
        public HistTripSegmentContainerMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            IntegerProperty(x => x.HistSeqNo)
               .IsId()
               .DisplayName("History Seq Number");

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber)
                .IsId()
                .DisplayName("Trip Seg Number");

            IntegerProperty(x => x.TripSegContainerSeqNumber)
                .IsId()
                .DisplayName("Trip Seg Container Seq Number")
                .AbbreviatedName("SeqNumber");

            StringProperty(x => x.TripSegContainerNumber);
            StringProperty(x => x.TripSegContainerType);
            StringProperty(x => x.TripSegContainerSize);
            StringProperty(x => x.TripSegContainerCommodityCode);
            StringProperty(x => x.TripSegContainerCommodityDesc);
            StringProperty(x => x.TripSegContainerLocation);
            StringProperty(x => x.TripSegContainerShortTerm);
            IntegerProperty(x => x.TripSegContainerWeightGross);
            IntegerProperty(x => x.TripSegContainerWeightGross2nd);
            IntegerProperty(x => x.TripSegContainerWeightTare);
            StringProperty(x => x.TripSegContainerReviewFlag);
            StringProperty(x => x.TripSegContainerReviewReason);
            DateProperty(x => x.TripSegContainerActionDateTime);
            StringProperty(x => x.TripSegContainerEntryMethod);
            DateProperty(x => x.WeightGrossDateTime);
            DateProperty(x => x.WeightGross2ndDateTime);
            DateProperty(x => x.WeightTareDateTime);
            IntegerProperty(x => x.TripSegContainerLevel);
            IntegerProperty(x => x.TripSegContainerLatitude);
            IntegerProperty(x => x.TripSegContainerLongitude);
            StringProperty(x => x.TripSegContainerLoaded);
            StringProperty(x => x.TripSegContainerOnTruck);
            StringProperty(x => x.TripScaleReferenceNumber);
            StringProperty(x => x.TripSegContainerSubReason);
            StringProperty(x => x.TripSegContainerComment);
            StringProperty(x => x.TripSegContainerComplete);

            ViewDefaults()
                .Property(x => x.HistSeqNo)
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.TripSegContainerSeqNumber)
                .Property(x => x.TripSegContainerNumber)
                .Property(x => x.TripSegContainerType)
                .Property(x => x.TripSegContainerSize)
                .Property(x => x.TripSegContainerCommodityCode)
                .Property(x => x.TripSegContainerCommodityDesc)
                .Property(x => x.TripSegContainerLocation)
                .Property(x => x.TripSegContainerShortTerm)
                .Property(x => x.TripSegContainerWeightGross)
                .Property(x => x.TripSegContainerWeightGross2nd)
                .Property(x => x.TripSegContainerWeightTare)
                .Property(x => x.TripSegContainerReviewFlag)
                .Property(x => x.TripSegContainerReviewReason)
                .Property(x => x.TripSegContainerActionDateTime)
                .Property(x => x.TripSegContainerEntryMethod)
                .Property(x => x.WeightGrossDateTime)
                .Property(x => x.WeightGross2ndDateTime)
                .Property(x => x.WeightTareDateTime)
                .Property(x => x.TripSegContainerLevel)
                .Property(x => x.TripSegContainerLatitude)
                .Property(x => x.TripSegContainerLongitude)
                .Property(x => x.TripSegContainerLoaded)
                .Property(x => x.TripSegContainerOnTruck)
                .Property(x => x.TripScaleReferenceNumber)
                .Property(x => x.TripSegContainerSubReason)
                .Property(x => x.TripSegContainerComment)
                .Property(x => x.TripSegContainerComplete)

                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.HistSeqNo)
                .OrderBy(x => x.TripSegNumber)
                .OrderBy(x => x.TripSegContainerSeqNumber);

        }
    }
}
