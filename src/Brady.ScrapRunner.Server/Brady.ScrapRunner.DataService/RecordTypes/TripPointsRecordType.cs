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
    
    [CreateAction("TripPoints")]
    [EditAction("TripPoints")]
    [DeleteAction("TripPoints")]
    public class TripPointsRecordType :
         ChangeableRecordType<TripPoints, string, TripPointsValidator, TripPointsDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripPoints, TripPoints>();
        }

        public override TripPoints GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripPoints
            {
                TripPointsHostCode1 = identityValues[0],
                TripPointsHostCode2 = identityValues[1],
            };
        }

        public override Expression<Func<TripPoints, bool>> GetIdentityPredicate(TripPoints item)
        {
            return x => x.TripPointsHostCode1 == item.TripPointsHostCode1 &&
                        x.TripPointsHostCode2 == item.TripPointsHostCode2;
        }

        public override Expression<Func<TripPoints, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripPointsHostCode1 == identityValues[0] &&
                        x.TripPointsHostCode2 == identityValues[1];
        }
    }
}
