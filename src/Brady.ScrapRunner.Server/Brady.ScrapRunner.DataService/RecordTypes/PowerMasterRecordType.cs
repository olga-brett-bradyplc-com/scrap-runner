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

    [CreateAction("PowerMaster")]
    [EditAction("PowerMaster")]
    [DeleteAction("PowerMaster")]
    public class PowerMasterRecordType :
        ChangeableRecordType<PowerMaster, string, PowerMasterValidator, PowerMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<PowerMaster, PowerMaster>();
        }

        public override PowerMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new PowerMaster
            {
                PowerId = identityValues[0],
            };
        }

        public override Expression<Func<PowerMaster, bool>> GetIdentityPredicate(PowerMaster item)
        {
            return x => x.PowerId == item.PowerId;
        }

        public override Expression<Func<PowerMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.PowerId == identityValues[0];
        }
    }

}
