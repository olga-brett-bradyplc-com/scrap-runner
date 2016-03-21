using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("CustomerMaster")]
    [EditAction("CustomerMaster")]
    [DeleteAction("CustomerMaster")]
    public class CustomerMasterRecordType :
        ChangeableRecordType<CustomerMaster, string, CustomerMasterValidator, CustomerMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<CustomerMaster, CustomerMaster>();
        }
    }
}