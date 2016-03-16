using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("Trip")]
    [EditAction("Trip")]
    [DeleteAction("Trip")]
    public class TripRecordType :
        ChangeableRecordType<Trip, string, TripValidator, TripDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<Trip, Trip>();
        }
    }
}