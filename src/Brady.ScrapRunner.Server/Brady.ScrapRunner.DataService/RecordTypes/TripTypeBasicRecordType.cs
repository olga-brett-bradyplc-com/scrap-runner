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

    [CreateAction("TripTypeBasic")]
    [EditAction("TripTypeBasic")]
    [DeleteAction("TripTypeBasic")]
    public class TripTypeBasicRecordType :
        ChangeableRecordType<TripTypeBasic, long, string, TripTypeBasicValidator, TripTypeBasicDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeBasic, TripTypeBasic>();
        }

        public override TripTypeBasic GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripTypeBasic
            {
                TripTypeCode = identityValues[0]
            };
        }

        public override Expression<Func<TripTypeBasic, bool>> GetIdentityPredicate(TripTypeBasic item)
        {
            return x => x.TripTypeCode == item.TripTypeCode;
        }

        public override Expression<Func<TripTypeBasic, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripTypeCode == identityValues[0] ;
        }
    }
}