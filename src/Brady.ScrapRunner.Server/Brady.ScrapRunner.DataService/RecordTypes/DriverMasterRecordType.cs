using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("DriverMaster")]
    [EditAction("DriverMaster")]
    [DeleteAction("DriverMaster")]
    public class DriverMasterRecordType :
         ChangeableRecordType<DriverMaster, string, DriverMasterValidator, DriverMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<DriverMaster, DriverMaster>();
        }
    }
}
