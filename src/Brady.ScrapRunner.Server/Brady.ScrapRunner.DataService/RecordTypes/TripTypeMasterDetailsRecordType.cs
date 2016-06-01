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

namespace Brady.ScrapRunner.DataService.Mappings
{

    [CreateAction("TripTypeMasterDetails")]
    [EditAction("TripTypeMasterDetails")]
    [DeleteAction("TripTypeMasterDetails")]
    public class TripTypeMasterDetailsRecordType :
          ChangeableRecordType<TripTypeMasterDetails, string, TripTypeMasterDetailsValidator, TripTypeMasterDetailsDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeMasterDetails, TripTypeMasterDetails>();
        }

        public override Expression<Func<TripTypeMasterDetails, bool>> GetIdentityPredicate(TripTypeMasterDetails item)
        {
            return x => x.AccessorialCode == item.AccessorialCode && 
                        x.TripTypeCode == item.TripTypeCode &&
                        x.TripTypeSeqNumber == item.TripTypeSeqNumber;
        }

        public override Expression<Func<TripTypeMasterDetails, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.AccessorialCode == identityValues[0] && 
                        x.TripTypeCode == identityValues[1] &&
                        x.TripTypeSeqNumber == int.Parse(identityValues[2]);
        }
    }
}
