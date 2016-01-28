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

    [CreateAction("CustomerMaster")]
    [EditAction("CustomerMaster")]
    [DeleteAction("CustomerMaster")]
    public class CustomerMasterRecordType :
        ChangeableRecordType<CustomerMaster, long, string, CustomerMasterValidator, CustomerMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CustomerMaster, CustomerMaster>();
        }

        public override CustomerMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CustomerMaster
            {
                CustHostCode = identityValues[0]
            };
        }

        public override Expression<Func<CustomerMaster, bool>> GetIdentityPredicate(CustomerMaster item)
        {
            return x => x.CustHostCode == item.CustHostCode;
        }

        public override Expression<Func<CustomerMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CustHostCode == identityValues[0];
        }

    }
}