using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("GPSLocation")]
    [EditAction("GPSLocation")]
    [DeleteAction("GPSLocation")]
    public class GPSLocationRecordType :
        ChangeableRecordType<GPSLocation, int, GPSLocationValidator, GPSLocationDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<GPSLocation, GPSLocation>();
        }
    }
}