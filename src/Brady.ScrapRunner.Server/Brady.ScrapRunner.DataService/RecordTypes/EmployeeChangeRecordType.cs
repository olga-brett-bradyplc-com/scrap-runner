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
    [CreateAction("EmployeeChange")]
    [EditAction("EmployeeChange")]
    [DeleteAction("EmployeeChange")]
    public class EmployeeChangeRecordType :
        ChangeableRecordType<EmployeeChange, string, EmployeeChangeValidator, EmployeeChangeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<EmployeeChange, EmployeeChange>();
        }

        public override Expression<Func<EmployeeChange, bool>> GetIdentityPredicate(EmployeeChange item)
        {
            return x => x.ActionFlag == item.ActionFlag &&
                        x.EmployeeId == item.EmployeeId &&
                        x.LoginId == item.LoginId &&
                        x.Password == item.Password &&
                        x.RegionId == item.RegionId;
        }

        public override Expression<Func<EmployeeChange, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.ActionFlag == identityValues[0] &&
                        x.EmployeeId == identityValues[1] &&
                        x.LoginId == identityValues[2] &&
                        x.Password == identityValues[3] &&
                        x.RegionId == identityValues[4];
        }
    }
}
