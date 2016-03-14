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

    [CreateAction("RegionMaster")]
    [EditAction("RegionMaster")]
    [DeleteAction("RegionMaster")]
    public class RegionMasterRecordType :
        ChangeableRecordType<RegionMaster, string, RegionMasterValidator, RegionMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<RegionMaster, RegionMaster>();
        }

        public override RegionMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new RegionMaster
            {
                RegionId = identityValues[0]
            };
        }

        public override Expression<Func<RegionMaster, bool>> GetIdentityPredicate(RegionMaster item)
        {
            return x => x.RegionId == item.RegionId;
        }

        public override Expression<Func<RegionMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.RegionId == identityValues[0];
        }

    }
}