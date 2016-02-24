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

    [CreateAction("DriverMaster")]
    [EditAction("DriverMaster")]
    [DeleteAction("DriverMaster")]
    public class DriverMasterRecordType :
         ChangeableRecordType<DriverMaster, string, DriverMasterValidator, DriverMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverMaster, DriverMaster>();
        }

        public override DriverMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new DriverMaster
            {
                EmployeeId = identityValues[0],
            };
        }

        public override Expression<Func<DriverMaster, bool>> GetIdentityPredicate(DriverMaster item)
        {
            return x => x.EmployeeId == item.EmployeeId;
        }

        public override Expression<Func<DriverMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.EmployeeId == identityValues[0];
        }
    }
}
