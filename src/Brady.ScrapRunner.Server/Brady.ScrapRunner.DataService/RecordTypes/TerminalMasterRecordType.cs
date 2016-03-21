using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{
    [CreateAction("TerminalMaster")]
    [EditAction("TerminalMaster")]
    [DeleteAction("TerminalMaster")]
    public class TerminalMasterRecordType :
        ChangeableRecordType<TerminalMaster, string, TerminalMasterValidator, TerminalMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TerminalMaster, TerminalMaster>();
        }
    }
}
