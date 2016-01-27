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

    [CreateAction("Trip")]
    [EditAction("Trip")]
    [DeleteAction("Trip")]
    public class TripRecordType :
        ChangeableRecordType<Trip, long, string, TripValidator, TripDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<Trip, Trip>();
        }

        public override Trip GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new Trip
            {
                TripNumber = identityValues[0]
            };
        }

        public override Expression<Func<Trip, bool>> GetIdentityPredicate(Trip item)
        {
            return x => x.TripNumber == item.TripNumber ;
        }

        public override Expression<Func<Trip, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripNumber == identityValues[0] ;
        }
    }
}