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

    [CreateAction("EmployeeMaster")]
    [EditAction("EmployeeMaster")]
    [DeleteAction("EmployeeMaster")]
    public class EmployeeMasterRecordType :
        ChangeableRecordType<EmployeeMaster, string, EmployeeMasterValidator, EmployeeMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<EmployeeMaster, EmployeeMaster>();
        }

        public override EmployeeMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new EmployeeMaster
            {
                EmployeeId = identityValues[0],
            };
        }

        public override Expression<Func<EmployeeMaster, bool>> GetIdentityPredicate(EmployeeMaster item)
        {
            return x => x.EmployeeId == item.EmployeeId;
        }

        public override Expression<Func<EmployeeMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.EmployeeId == identityValues[0] ;
        }
    }

}
