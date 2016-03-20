using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("RegionMaster")]
    [EditAction("RegionMaster")]
    [DeleteAction("RegionMaster")]
    public class RegionMasterRecordType :
        ChangeableRecordType<RegionMaster, string, RegionMasterValidator, RegionMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<RegionMaster, RegionMaster>();
        }
    }
}