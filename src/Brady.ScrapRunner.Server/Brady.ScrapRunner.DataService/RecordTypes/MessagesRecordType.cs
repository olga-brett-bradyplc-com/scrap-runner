using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("Messages")]
    [EditAction("Messages")]
    [DeleteAction("Messages")]

    public class MessagesRecordType :
        ChangeableRecordType<Messages, int, MessagesValidator, MessagesDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<Messages, Messages>();
        }
    }
}
