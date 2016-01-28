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

    [CreateAction("TerminalChange")]
    [EditAction("TerminalChange")]
    [DeleteAction("TerminalChange")]
    public class TerminalChangeRecordType :
        ChangeableRecordType<TerminalChange, long, string, TerminalChangeValidator, TerminalChangeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TerminalChange, TerminalChange>();
        }

        public override TerminalChange GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TerminalChange
            {
                TerminalId = identityValues[0]
            };
        }

        public override Expression<Func<TerminalChange, bool>> GetIdentityPredicate(TerminalChange item)
        {
            return x => x.TerminalId == item.TerminalId;
        }

        public override Expression<Func<TerminalChange, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TerminalId == identityValues[0];
        }

    }
}