using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Mappings
{
    public class TripSegmentContainerMap : ClassMapping<TripSegmentContainer>
    {
        public TripSegmentContainerMap()
        {
            Table("TripSegmentContainer");

            ComposedId(map =>
            {
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegContainerSeqNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripNumber, ';', TripSegContainerSeqNumber, ';',TripSegNumber )");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripSegContainerNumber);
            Property(x => x.TripSegContainerType);
            Property(x => x.TripSegContainerSize);
            Property(x => x.TripSegContainerCommodityCode);
            Property(x => x.TripSegContainerCommodityDesc);
            Property(x => x.TripSegContainerLocation);
            Property(x => x.TripSegContainerShortTerm);
            Property(x => x.TripSegContainerWeightGross);
            Property(x => x.TripSegContainerWeightGross2nd);
            Property(x => x.TripSegContainerWeightTare);
            Property(x => x.TripSegContainerReviewFlag);
            Property(x => x.TripSegContainerReviewReason);
            Property(x => x.TripSegContainerActionDateTime);
            Property(x => x.TripSegContainerEntryMethod);
            Property(x => x.WeightGrossDateTime);
            Property(x => x.WeightGross2ndDateTime);
            Property(x => x.WeightTareDateTime);
            Property(x => x.TripSegContainerLevel);
            Property(x => x.TripSegContainerLatitude);
            Property(x => x.TripSegContainerLongitude);
            Property(x => x.TripSegContainerLoaded);
            Property(x => x.TripSegContainerOnTruck);
            Property(x => x.TripScaleReferenceNumber);
            Property(x => x.TripSegContainerSubReason);
            Property(x => x.TripSegContainerComment);
            Property(x => x.TripSegContainerComplete);

        }
    }
}
