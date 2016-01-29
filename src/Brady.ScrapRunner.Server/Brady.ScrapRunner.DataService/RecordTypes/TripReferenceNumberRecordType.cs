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

    [CreateAction("TripReferenceNumber")]
    [EditAction("TripReferenceNumber")]
    [DeleteAction("TripReferenceNumber")]
    public class TripReferenceNumberRecordType :
        ChangeableRecordType<TripReferenceNumber, long, string, TripReferenceNumberValidator, TripReferenceNumberDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripReferenceNumber, TripReferenceNumber>();
        }

        public override TripReferenceNumber GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripReferenceNumber
            {
                TripNumber = identityValues[0], 
                TripSeqNumber = int.Parse(identityValues[1])
            };
        }

        public override Expression<Func<TripReferenceNumber, bool>> GetIdentityPredicate(TripReferenceNumber item)
        {
            return x => x.TripNumber == item.TripNumber &&
                        x.TripSeqNumber == item.TripSeqNumber ;
        }

        public override Expression<Func<TripReferenceNumber, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripNumber == identityValues[0] &&
                        x.TripSeqNumber == int.Parse(identityValues[1]);
        }
    }
}