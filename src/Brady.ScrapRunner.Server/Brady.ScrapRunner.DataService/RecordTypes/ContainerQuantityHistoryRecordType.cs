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
    [CreateAction("ContainerQuantityHistory")]
    [EditAction("ContainerQuantityHistory")]
    [DeleteAction("ContainerQuantityHistory")]
    public class ContainerQuantityHistoryRecordType :
         ChangeableRecordType<ContainerQuantityHistory, string, ContainerQuantityHistoryValidator, ContainerQuantityHistoryDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<ContainerQuantityHistory, ContainerQuantityHistory>();
        }

        public override Expression<Func<ContainerQuantityHistory, bool>> GetIdentityPredicate(ContainerQuantityHistory item)
        {
            return x => x.CustHostCode == item.CustHostCode &&
                        x.CustSeqNo == item.CustSeqNo;
        }

        public override Expression<Func<ContainerQuantityHistory, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CustHostCode == identityValues[0] &&
                        x.CustSeqNo == int.Parse(identityValues[1]);
        }
    }
}
