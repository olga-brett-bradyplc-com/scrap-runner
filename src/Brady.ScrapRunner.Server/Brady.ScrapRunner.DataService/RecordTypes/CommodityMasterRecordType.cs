using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("CommodityMaster")]
    [EditAction("CommodityMaster")]
    [DeleteAction("CommodityMaster")]
    public class CommodityMasterRecordType :
        ChangeableRecordType<CommodityMaster, string, CommodityMasterValidator, CommodityMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<CommodityMaster, CommodityMaster>();
        }
    }
}