using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("TripTypeAllowCustType")]
    [EditAction("TripTypeAllowCustType")]
    [DeleteAction("TripTypeAllowCustType")]
    public class TripTypeAllowCustTypeRecordType :
           ChangeableRecordType<TripTypeAllowCustType, string, TripTypeAllowCustTypeValidator, TripTypeAllowCustTypeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeAllowCustType, TripTypeAllowCustType>();
        }

        public override TripTypeAllowCustType GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripTypeAllowCustType
            {
                TripTypeCode = identityValues[0],
                TripTypeCustType = identityValues[1],
                TripTypeSeqNumber = int.Parse(identityValues[2])
            };
        }

        public override Expression<Func<TripTypeAllowCustType, bool>> GetIdentityPredicate(TripTypeAllowCustType item)
        {
            return x => x.TripTypeCode == item.TripTypeCode &&
                   x.TripTypeCustType == item.TripTypeCode &&
                   x.TripTypeSeqNumber == item.TripTypeSeqNumber;
        }

        public override Expression<Func<TripTypeAllowCustType, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripTypeCode == identityValues[0] &&
                        x.TripTypeCustType == identityValues[1] && 
                        x.TripTypeSeqNumber == int.Parse(identityValues[2]);
        }
    }
}
