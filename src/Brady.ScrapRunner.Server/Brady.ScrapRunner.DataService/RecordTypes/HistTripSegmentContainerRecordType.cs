using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.RecordTypes
{
    [CreateAction("HistTripSegmentContainer")]
    [EditAction("HistTripSegmentContainer")]
    [DeleteAction("HistTripSegmentContainer")]
    public class HistTripSegmentContainerRecordType :
        ChangeableRecordType<HistTripSegmentContainer, string, HistTripSegmentContainerValidator, HistTripSegmentContainerDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<HistTripSegmentContainer, HistTripSegmentContainer>();
        }

        public override HistTripSegmentContainer GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new HistTripSegmentContainer
            {
                HistSeqNo = int.Parse(identityValues[0]),
                TripNumber = identityValues[1],
                TripSegContainerSeqNumber = int.Parse(identityValues[2]),
                TripSegNumber = identityValues[3]
            };

        }

        public override Expression<Func<HistTripSegmentContainer, bool>> GetIdentityPredicate(HistTripSegmentContainer item)
        {
            return x => x.HistSeqNo == item.HistSeqNo &&
                        x.TripNumber == item.TripNumber &&
                        x.TripSegContainerSeqNumber == item.TripSegContainerSeqNumber &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<HistTripSegmentContainer, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);

            return x => x.HistSeqNo == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1] &&
                        x.TripSegContainerSeqNumber == int.Parse(identityValues[2]) &&
                        x.TripSegNumber == identityValues[3];

        }
    }
}
