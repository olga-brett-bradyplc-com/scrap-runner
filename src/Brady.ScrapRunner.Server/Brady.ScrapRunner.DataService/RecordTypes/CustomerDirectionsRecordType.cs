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
    [CreateAction("CustomerDirections")]
    [EditAction("CustomerDirections")]
    [DeleteAction("CustomerDirections")]
    public class CustomerDirectionsRecordType :
        ChangeableRecordType<CustomerDirections,string,CustomerDirectionsValidator,CustomerDirectionsDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CustomerDirections, CustomerDirections>();
        }

        public override CustomerDirections GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CustomerDirections
            {
                CustHostCode = identityValues[0],
                DirectionsSeqNo = int.Parse(identityValues[1])
            };
        }

        public override Expression<Func<CustomerDirections, bool>> GetIdentityPredicate(CustomerDirections item)
        {
            return x => x.CustHostCode == item.CustHostCode &&
                        x.DirectionsSeqNo == item.DirectionsSeqNo;
        }

        public override Expression<Func<CustomerDirections, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CustHostCode == identityValues[0] &&
                        x.DirectionsSeqNo == int.Parse(identityValues[1]);
        }
    }
}

