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

    [CreateAction("Preference")]
    [EditAction("Preference")]
    [DeleteAction("Preference")]
    public class PreferenceRecordType :
        ChangeableRecordType<Preference, string, PreferenceValidator, PreferenceDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<Preference, Preference>();
        }

        public override Preference GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new Preference
            {
                TerminalId = identityValues[0],
                Parameter = identityValues[1]
            };
        }

        public override Expression<Func<Preference, bool>> GetIdentityPredicate(Preference item)
        {
            return x => x.TerminalId == item.TerminalId && 
                x.Parameter == item.Parameter;
        }
        public override Expression<Func<Preference, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TerminalId == identityValues[0] &&
                        x.Parameter == identityValues[1];
        }
    }
}