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

    [CreateAction("CommodityMasterDest")]
    [EditAction("CommodityMasterDest")]
    [DeleteAction("CommodityMasterDest")]
    public class CommodityMasterDestRecordType :
        ChangeableRecordType<CommodityMasterDest, string, CommodityMasterDestValidator, CommodityMasterDestDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CommodityMasterDest, CommodityMasterDest>();
        }

        public override CommodityMasterDest GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CommodityMasterDest
            {
                CommodityCode = identityValues[0],
                DestTerminalId = identityValues[1]
            };
        }

        public override Expression<Func<CommodityMasterDest, bool>> GetIdentityPredicate(CommodityMasterDest item)
        {
            return x => x.CommodityCode == item.CommodityCode &&
                        x.DestTerminalId == item.DestTerminalId;
        }

        public override Expression<Func<CommodityMasterDest, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CommodityCode == identityValues[0] &&
                        x.DestTerminalId == identityValues[1];
        }

    }
}
