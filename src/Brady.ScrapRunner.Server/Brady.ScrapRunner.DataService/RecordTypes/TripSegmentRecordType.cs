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

    [CreateAction("TripSegment")]
    [EditAction("TripSegment")]
    [DeleteAction("TripSegment")]
    public class TripSegmentRecordType :
        ChangeableRecordType<TripSegment, long, string, TripSegmentValidator, TripSegmentDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripSegment, TripSegment>();
        }

        public override TripSegment GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripSegment
            {
                TripNumber = identityValues[0],
                TripSegNumber = identityValues[1]
            };
        }

        public override Expression<Func<TripSegment, bool>> GetIdentityPredicate(TripSegment item)
        {
            return x => x.TripNumber == item.TripNumber &&
                x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<TripSegment, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripNumber == identityValues[0] &&
                        x.TripSegNumber == identityValues[1];
        }
    }
}