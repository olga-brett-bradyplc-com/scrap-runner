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
 
    [CreateAction("TripSegmentTime")]
    [EditAction("TripSegmentTime")]
    [DeleteAction("TripSegmentTime")]
    public class TripSegmentTimeRecordType :
        ChangeableRecordType<TripSegmentTime, string, TripSegmentTimeValidator, TripSegmentTimeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripSegmentTime, TripSegmentTime>();
        }

        public override TripSegmentTime GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripSegmentTime
            {
                SeqNumber = int.Parse(identityValues[0]),
                TripNumber = identityValues[1],
                TripSegNumber = identityValues[2]
            };
        }

        public override Expression<Func<TripSegmentTime, bool>> GetIdentityPredicate(TripSegmentTime item)
        {
            return x => x.SeqNumber == item.SeqNumber && 
                        x.TripNumber == item.TripNumber &&                   
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<TripSegmentTime, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.SeqNumber == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1] && 
                        x.TripSegNumber == identityValues[2];
        }
    }
}
