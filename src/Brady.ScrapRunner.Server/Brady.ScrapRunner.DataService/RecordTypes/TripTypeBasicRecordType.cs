using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("TripTypeBasic")]
    [EditAction("TripTypeBasic")]
    [DeleteAction("TripTypeBasic")]
    public class TripTypeBasicRecordType :
        ChangeableRecordType<TripTypeBasic, string, TripTypeBasicValidator, TripTypeBasicDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TripTypeBasic, TripTypeBasic>();
        }
    }
}