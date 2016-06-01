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
 
    [CreateAction("SecurityMaster")]
    [EditAction("SecurityMaster")]
    [DeleteAction("SecurityMaster")]
    public class SecurityMasterRecordType :
        ChangeableRecordType<SecurityMaster, string, SecurityMasterValidator, SecurityMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<SecurityMaster, SecurityMaster>();
        }

        public override Expression<Func<SecurityMaster, bool>> GetIdentityPredicate(SecurityMaster item)
        {
            return x => x.SecurityFunction == item.SecurityFunction &&
                        x.SecurityLevel == item.SecurityLevel &&
                        x.SecurityProgram == item.SecurityProgram &&
                        x.SecurityType == item.SecurityType;
        }

        public override Expression<Func<SecurityMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.SecurityFunction == identityValues[0] &&
                        x.SecurityLevel == identityValues[1] &&
                        x.SecurityProgram == identityValues[2] &&
                        x.SecurityType == identityValues[3];
        }
    }
}
