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

    [CreateAction("HistTripSegmentMileage")]
    [EditAction("HistTripSegmentMileage")]
    [DeleteAction("HistTripSegmentMileage")]
    public class HistTripSegmentMileageRecordType :
        ChangeableRecordType<HistTripSegmentMileage, string, HistTripSegmentMileageValidator, HistTripSegmentMileageDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<HistTripSegmentMileage, HistTripSegmentMileage>();
        }

        public override HistTripSegmentMileage GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new HistTripSegmentMileage
            {
                HistSeqNo = int.Parse(identityValues[0]),
                TripNumber = identityValues[1],
                TripSegMileageSeqNumber = int.Parse(identityValues[2]),
                TripSegNumber = identityValues[3]
            };
        }

        public override Expression<Func<HistTripSegmentMileage, bool>> GetIdentityPredicate(HistTripSegmentMileage item)
        {
            return x => x.HistSeqNo == item.HistSeqNo &&
                        x.TripNumber == item.TripNumber &&
                        x.TripSegMileageSeqNumber == item.TripSegMileageSeqNumber &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<HistTripSegmentMileage, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.HistSeqNo == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1] &&
                        x.TripSegMileageSeqNumber == int.Parse(identityValues[2]) &&
                        x.TripSegNumber == identityValues[3];
        }
    }
}
