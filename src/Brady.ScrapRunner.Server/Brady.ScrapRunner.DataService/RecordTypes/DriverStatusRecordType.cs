using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Brady.ScrapRunner.DataService.Validators;


namespace Brady.ScrapRunner.DataService.RecordTypes
{
    [CreateAction("DriverStatus")]
    [EditAction("DriverStatus")]
    [DeleteAction("DriverStatus")]
    public class DriverStatusRecordType :
        ChangeableRecordType<DriverStatus, string, DriverStatusValidator, DriverStatusDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverStatus, DriverStatus>();
        }

        public override DriverStatus GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new DriverStatus
            {
                EmployeeId = identityValues[0],
            };
        }

        public override Expression<Func<DriverStatus, bool>> GetIdentityPredicate(DriverStatus item)
        {
            return x => x.EmployeeId == item.EmployeeId;
        }

        public override Expression<Func<DriverStatus, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.EmployeeId == identityValues[0] ;
        }
    }

}
