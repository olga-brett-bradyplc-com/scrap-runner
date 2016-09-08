using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("PreferencesDefault")]
    [EditAction("PreferencesDefault")]
    [DeleteAction("PreferencesDefault")]
    public class PreferencesDefaultRecordType :
        ChangeableRecordType<PreferencesDefault, string, PreferencesDefaultValidator, PreferencesDefaultDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<PreferencesDefault, PreferencesDefault>();
        }
    }
}
