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
 
    [CreateAction("TripTypeMaster")]
    [EditAction("TripTypeMaster")]
    [DeleteAction("TripTypeMaster")]
    public class TripTypeMasterRecordType :
          ChangeableRecordType<TripTypeMaster, string, TripTypeMasterValidator, TripTypeMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeMaster, TripTypeMaster>();
        }

        public override Expression<Func<TripTypeMaster, bool>> GetIdentityPredicate(TripTypeMaster item)
        {
            return x => x.TripTypeCode == item.TripTypeCode &&
                        x.TripTypeSeqNumber == item.TripTypeSeqNumber;
        }

        public override Expression<Func<TripTypeMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripTypeCode == identityValues[0] &&
                        x.TripTypeSeqNumber == int.Parse(identityValues[1]);
        }
    }
}
