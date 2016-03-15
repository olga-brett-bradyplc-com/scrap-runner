using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("EmployeeMaster")]
    [EditAction("EmployeeMaster")]
    [DeleteAction("EmployeeMaster")]
    public class EmployeeMasterRecordType :
        ChangeableRecordType<EmployeeMaster, string, EmployeeMasterValidator, EmployeeMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<EmployeeMaster, EmployeeMaster>();
        }
    }
}
