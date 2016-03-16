using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("TerminalChange")]
    [EditAction("TerminalChange")]
    [DeleteAction("TerminalChange")]
    public class TerminalChangeRecordType :
        ChangeableRecordType<TerminalChange, string, TerminalChangeValidator, TerminalChangeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TerminalChange, TerminalChange>();
        }
    }
}