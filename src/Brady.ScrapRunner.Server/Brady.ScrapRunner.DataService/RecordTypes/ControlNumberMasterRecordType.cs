using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

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
            Mapper.CreateMap<ControlNumberMaster, ControlNumberMaster>();
        }
    }
}
