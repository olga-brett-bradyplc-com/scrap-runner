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

    [CreateAction("EmployeePreferences")]
    [EditAction("EmployeePreferences")]
    [DeleteAction("EmployeePreferences")]
    public class EmployeePreferencesRecordType :
        ChangeableRecordType<EmployeePreferences, string, EmployeePreferencesValidator, EmployeePreferencesDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<EmployeePreferences, EmployeePreferences>();
        }

        public override Expression<Func<EmployeePreferences, bool>> GetIdentityPredicate(EmployeePreferences item)
        {
            return x => x.RegionId == item.RegionId &&
                        x.TerminalId == item.TerminalId &&
                        x.EmployeeId == item.EmployeeId &&
                        x.Parameter == item.Parameter;
        }

        public override Expression<Func<EmployeePreferences, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.RegionId == identityValues[0] &&
                        x.TerminalId == identityValues[1] &&
                        x.EmployeeId == identityValues[2] &&
                        x.Parameter == identityValues[3];
        }
    }
}
