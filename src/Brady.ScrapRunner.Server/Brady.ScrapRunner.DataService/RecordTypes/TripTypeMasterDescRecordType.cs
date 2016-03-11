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
 
    [CreateAction("TripTypeMasterDesc")]
    [EditAction("TripTypeMasterDesc")]
    [DeleteAction("TripTypeMasterDesc")]
    public class TripTypeMasterDescRecordType :
         ChangeableRecordType<TripTypeMasterDesc, string, TripTypeMasterDescValidator, TripTypeMasterDescDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeMasterDesc, TripTypeMasterDesc>();
        }

        public override TripTypeMasterDesc GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripTypeMasterDesc
            {
                TripTypeCode = identityValues[0]
            };
        }

        public override Expression<Func<TripTypeMasterDesc, bool>> GetIdentityPredicate(TripTypeMasterDesc item)
        {
            return x => x.TripTypeCode == item.TripTypeCode;
        }

        public override Expression<Func<TripTypeMasterDesc, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripTypeCode == identityValues[0];
        }
    }
}
