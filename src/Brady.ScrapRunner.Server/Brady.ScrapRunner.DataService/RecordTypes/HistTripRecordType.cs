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
    [CreateAction("HistTrip")]
    [EditAction("HistTrip")]
    [DeleteAction("HistTrip")]
    public class HistTripRecordType :
         ChangeableRecordType<HistTrip, string, HistTripValidator, HistTripDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<HistTrip, HistTrip>();
        }

        public override HistTrip GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new HistTrip
            {
                HistSeqNo = int.Parse(identityValues[0]),
                TripNumber = identityValues[1]
            };
        }

        public override Expression<Func<HistTrip, bool>> GetIdentityPredicate(HistTrip item)
        {
            return x => x.HistSeqNo == item.HistSeqNo && 
                        x.TripNumber == item.TripNumber;
        }

        public override Expression<Func<HistTrip, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.HistSeqNo == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1];
        }
    }
}
