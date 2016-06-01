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
 
    [CreateAction("PowerLimits")]
    [EditAction("PowerLimits")]
    [DeleteAction("PowerLimits")]
    public class PowerLimitsRecordType :
        ChangeableRecordType<PowerLimits, string, PowerLimitsValidator, PowerLimitsDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<PowerLimits, PowerLimits>();
        }

        public override Expression<Func<PowerLimits, bool>> GetIdentityPredicate(PowerLimits item)
        {
            return x => x.ContainerType == item.ContainerType &&
                        x.PowerId == item.PowerId &&
                        x.PowerSeqNumber == item.PowerSeqNumber;
        }

        public override Expression<Func<PowerLimits, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.ContainerType == identityValues[0] &&
                        x.PowerId == identityValues[1] &&
                        x.PowerSeqNumber == int.Parse(identityValues[2]);
        }
    }
}
