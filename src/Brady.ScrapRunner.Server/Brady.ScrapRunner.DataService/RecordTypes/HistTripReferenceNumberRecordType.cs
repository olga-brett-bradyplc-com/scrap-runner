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
    [CreateAction("HistTripReferenceNumber")]
    [EditAction("HistTripReferenceNumber")]
    [DeleteAction("HistTripReferenceNumber")]
    public class HistTripReferenceNumberRecordType :
        ChangeableRecordType<HistTripReferenceNumber, string, HistTripReferenceNumberValidator, HistTripReferenceNumberDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<HistTripReferenceNumber, HistTripReferenceNumber>();
        }

        public override HistTripReferenceNumber GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new HistTripReferenceNumber
            {
                HistSeqNo = int.Parse(identityValues[0]),
                TripNumber = identityValues[1],
                TripSeqNumber = int.Parse(identityValues[2])
            };
        }

        public override Expression<Func<HistTripReferenceNumber, bool>> GetIdentityPredicate(HistTripReferenceNumber item)
        {
            return x => x.HistSeqNo == item.HistSeqNo &&
                        x.TripNumber == item.TripNumber &&
                        x.TripSeqNumber == item.TripSeqNumber;
        }

        public override Expression<Func<HistTripReferenceNumber, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.HistSeqNo == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1] &&
                        x.TripSeqNumber == int.Parse(identityValues[2]);
        }
    }
}
