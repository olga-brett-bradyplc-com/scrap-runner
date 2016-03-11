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
    [CreateAction("HistTripSegment")]
    [EditAction("HistTripSegment")]
    [DeleteAction("HistTripSegment")]
    public class HistTripSegmentRecordType :
        ChangeableRecordType<HistTripSegment, string, HistTripSegmentValidator, HistTripSegmentDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<HistTripSegment, HistTripSegment>();
        }

        public override HistTripSegment GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new HistTripSegment
            {
                HistSeqNo = int.Parse(identityValues[0]),
                TripNumber = identityValues[1],
                TripSegNumber = identityValues[2]
            };
        }

        public override Expression<Func<HistTripSegment, bool>> GetIdentityPredicate(HistTripSegment item)
        {
            return x => x.HistSeqNo == item.HistSeqNo &&
                        x.TripNumber == item.TripNumber &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<HistTripSegment, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.HistSeqNo == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1] &&
                        x.TripSegNumber == identityValues[2];
        }
    }
}
