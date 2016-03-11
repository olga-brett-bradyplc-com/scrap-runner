using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
            var mapping = Mapper.CreateMap<CommodityMaster, CommodityMaster>();
        }

        public override CommodityMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CommodityMaster
            {
                CommodityCode = identityValues[0]
            };
        }

        public override Expression<Func<CommodityMaster, bool>> GetIdentityPredicate(CommodityMaster item)
        {
            return x => x.CommodityCode == item.CommodityCode;
        }

        public override Expression<Func<CommodityMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CommodityCode == identityValues[0];
        }

    }
}