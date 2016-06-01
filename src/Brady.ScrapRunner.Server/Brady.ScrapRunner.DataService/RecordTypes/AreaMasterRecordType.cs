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
    [CreateAction("AreaMaster")]
    [EditAction("AreaMaster")]
    [DeleteAction("AreaMaster")]
    public class AreaMasterRecordType :
        ChangeableRecordType<AreaMaster,string,AreaMasterValidator,AreaMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<AreaMaster, AreaMaster>();
        }

        public override Expression<Func<AreaMaster,bool>> GetIdentityPredicate(AreaMaster item)
        {
            return x => x.AreaId == item.AreaId &&
                        x.TerminalId == item.TerminalId;
        }

        public override Expression<Func<AreaMaster,bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.AreaId == identityValues[0] &&
                        x.TerminalId == identityValues[1];
        }
    }
}
