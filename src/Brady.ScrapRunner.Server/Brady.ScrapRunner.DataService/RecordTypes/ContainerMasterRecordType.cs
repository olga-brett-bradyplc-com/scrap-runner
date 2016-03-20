using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("ContainerMaster")]
    [EditAction("ContainerMaster")]
    [DeleteAction("ContainerMaster")]
    public class ContainerMasterRecordType :
        ChangeableRecordType<ContainerMaster, string, ContainerMasterValidator, ContainerMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<ContainerMaster, ContainerMaster>();
        }
    }
}