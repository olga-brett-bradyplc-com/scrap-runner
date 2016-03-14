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

    [CreateAction("CustomerCommodity")]
    [EditAction("CustomerCommodity")]
    [DeleteAction("CustomerCommodity")]
    public class CustomerCommodityRecordType :
        ChangeableRecordType<CustomerCommodity, string, CustomerCommodityValidator, CustomerCommodityDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CustomerCommodity, CustomerCommodity>();
        }

        public override CustomerCommodity GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CustomerCommodity
            {
                CustCommodityCode = identityValues[0],
                CustHostCode = identityValues[1]
            };
        }

        public override Expression<Func<CustomerCommodity, bool>> GetIdentityPredicate(CustomerCommodity item)
        {
            return x => x.CustCommodityCode == item.CustCommodityCode &&
                        x.CustHostCode == item.CustHostCode;
        }
    
        public override Expression<Func<CustomerCommodity, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CustCommodityCode == identityValues[0] &&
                        x.CustHostCode == identityValues[1];
        }

    }
}