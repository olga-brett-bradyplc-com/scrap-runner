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
 
    [CreateAction("TripTypeBasicDetails")]
    [EditAction("TripTypeBasicDetails")]
    [DeleteAction("TripTypeBasicDetails")]
    public class TripTypeBasicDetailsRecordType :
        ChangeableRecordType<TripTypeBasicDetails, string, TripTypeBasicDetailsValidator, TripTypeBasicDetailsDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeBasicDetails, TripTypeBasicDetails>();
        }

        public override TripTypeBasicDetails GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripTypeBasicDetails
            {
                ContainerType = identityValues[0],
                SeqNo = int.Parse(identityValues[1]),
                TripTypeCode = identityValues[2]
            };
        }

        public override Expression<Func<TripTypeBasicDetails, bool>> GetIdentityPredicate(TripTypeBasicDetails item)
        {
            return x => x.ContainerType == item.ContainerType &&
                        x.SeqNo == item.SeqNo &&
                        x.TripTypeCode == item.TripTypeCode;
        }

        public override Expression<Func<TripTypeBasicDetails, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripTypeCode == identityValues[0] &&
                        x.SeqNo == int.Parse(identityValues[1]) &&
                        x.TripTypeCode == identityValues[2];
        }
    }
}
