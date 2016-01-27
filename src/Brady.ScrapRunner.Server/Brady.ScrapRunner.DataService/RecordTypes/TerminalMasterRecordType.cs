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
    [CreateAction("TerminalMaster")]
    [EditAction("TerminalMaster")]
    [DeleteAction("TerminalMaster")]
    public class TerminalMasterRecordType :
        ChangeableRecordType<TerminalMaster, long, string, TerminalMasterValidator, TerminalMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TerminalMaster, TerminalMaster>();
        }

        public override TerminalMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TerminalMaster
            {
                TerminalId = identityValues[0],
            };
        }

        public override Expression<Func<TerminalMaster, bool>> GetIdentityPredicate(TerminalMaster item)
        {
            return x => x.TerminalId == item.TerminalId;
        }

        public override Expression<Func<TerminalMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TerminalId == identityValues[0] ;
        }
    }

}
