using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{
 
    [CreateAction("TripTypeMasterDesc")]
    [EditAction("TripTypeMasterDesc")]
    [DeleteAction("TripTypeMasterDesc")]
    public class TripTypeMasterDescRecordType :
         ChangeableRecordType<TripTypeMasterDesc, string, TripTypeMasterDescValidator, TripTypeMasterDescDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<TripTypeMasterDesc, TripTypeMasterDesc>();
        }
    }
}
