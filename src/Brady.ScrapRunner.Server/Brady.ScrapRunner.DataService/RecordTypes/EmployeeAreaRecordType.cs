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
 
    [CreateAction("EmployeeArea")]
    [EditAction("EmployeeArea")]
    [DeleteAction("EmployeeArea")]
    public class EmployeeAreaRecordType :
        ChangeableRecordType<EmployeeArea, string, EmployeeAreaValidator, EmployeeAreaDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<EmployeeArea, EmployeeArea>();
        }
        public override EmployeeArea GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new EmployeeArea
            {
                AreaId = identityValues[0],
                EmployeeId = identityValues[1]
            };
        }
        public override Expression<Func<EmployeeArea, bool>> GetIdentityPredicate(EmployeeArea item)
        {
            return x => x.AreaId == item.AreaId &&
                        x.EmployeeId == item.EmployeeId;
        }

        public override Expression<Func<EmployeeArea, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.AreaId == identityValues[0] &&
                        x.EmployeeId == identityValues[1];
        }
    }
}
