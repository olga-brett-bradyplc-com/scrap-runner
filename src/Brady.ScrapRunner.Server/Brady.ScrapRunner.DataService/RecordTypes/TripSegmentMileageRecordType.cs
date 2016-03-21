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

    [CreateAction("TripSegmentMileage")]
    [EditAction("TripSegmentMileage")]
    [DeleteAction("TripSegmentMileage")]
    public class TripSegmentMileageRecordType :
        ChangeableRecordType<TripSegmentMileage, string, TripSegmentMileageValidator, TripSegmentMileageDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripSegmentMileage, TripSegmentMileage>();
        }

        public override TripSegmentMileage GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripSegmentMileage
            {
                TripNumber = identityValues[0],
                TripSegMileageSeqNumber = int.Parse(identityValues[1]), 
                TripSegNumber = identityValues[2] 
            };
        }

        public override Expression<Func<TripSegmentMileage, bool>> GetIdentityPredicate(TripSegmentMileage item)
        {
            return x => x.TripNumber == item.TripNumber &&
                        x.TripSegMileageSeqNumber == item.TripSegMileageSeqNumber &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<TripSegmentMileage, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripNumber == identityValues[0] &&
                        x.TripSegMileageSeqNumber == int.Parse(identityValues[1]) &&
                        x.TripSegNumber == identityValues[2] ;
        }
    }
}