using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("ContainerChange")]
    [EditAction("ContainerChange")]
    [DeleteAction("ContainerChange")]
    public class ContainerChangeRecordType :
        ChangeableRecordType<ContainerChange, string, ContainerChangeValidator, ContainerChangeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<ContainerChange, ContainerChange>();
        }
    }
}