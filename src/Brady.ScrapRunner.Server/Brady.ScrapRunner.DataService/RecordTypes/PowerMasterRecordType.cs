using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

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
            Mapper.CreateMap<PowerMaster, PowerMaster>();
        }
    }
}
