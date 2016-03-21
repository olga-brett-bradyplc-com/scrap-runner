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

    [CreateAction("ControlNumberMaster")]
    [EditAction("ControlNumberMaster")]
    [DeleteAction("ControlNumberMaster")]
    public class ControlNumberMasterRecordType :
        ChangeableRecordType<ControlNumberMaster, string, ControlNumberMasterValidator, ControlNumberMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<ControlNumberMaster, ControlNumberMaster>();
        }

        public override ControlNumberMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new ControlNumberMaster
            {
                ControlType = identityValues[0]
            };
        }

        public override Expression<Func<ControlNumberMaster, bool>> GetIdentityPredicate(ControlNumberMaster item)
        {
            return x => x.ControlType == item.ControlType;
        }

        public override Expression<Func<ControlNumberMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.ControlType == identityValues[0];
        }
    }
}
