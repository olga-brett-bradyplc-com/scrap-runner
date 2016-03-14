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

    [CreateAction("CustomerLocation")]
    [EditAction("CustomerLocation")]
    [DeleteAction("CustomerLocation")]
    public class CustomerLocationRecordType :
        ChangeableRecordType<CustomerLocation, string, CustomerLocationValidator, CustomerLocationDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CustomerLocation, CustomerLocation>();
        }

        public override CustomerLocation GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CustomerLocation
            {
                CustHostCode = identityValues[0], 
                CustLocation = identityValues[1]
            };
        }

        public override Expression<Func<CustomerLocation, bool>> GetIdentityPredicate(CustomerLocation item)
        {
            return x => x.CustHostCode == item.CustHostCode &&
                        x.CustLocation == item.CustLocation;
        }

        public override Expression<Func<CustomerLocation, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CustHostCode == identityValues[0] &&
                        x.CustLocation == identityValues[1];
        }

    }
}