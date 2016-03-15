using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{
    [CreateAction("DriverStatus")]
    [EditAction("DriverStatus")]
    [DeleteAction("DriverStatus")]
    public class DriverStatusRecordType :
        ChangeableRecordType<DriverStatus, string, DriverStatusValidator, DriverStatusDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverStatus, DriverStatus>();
        }
    }
}
