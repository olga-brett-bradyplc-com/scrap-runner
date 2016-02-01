using AutoMapper;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Brady.ScrapRunner.DataService.Validators;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("PowerHistory")]
    [EditAction("PowerHistory")]
    [DeleteAction("PowerHistory")]
    public class PowerHistoryRecordType :
        ChangeableRecordType<PowerHistory, long, string, PowerHistoryValidator, PowerHistoryDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<PowerHistory, PowerHistory>();
        }

        public override PowerHistory GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new PowerHistory
            {
                PowerId = identityValues[0],
                PowerSeqNumber = int.Parse(identityValues[1])
            };
        }

        public override Expression<Func<PowerHistory, bool>> GetIdentityPredicate(PowerHistory item)
        {
            return x => x.PowerId == item.PowerId &&
                        x.PowerSeqNumber == item.PowerSeqNumber;
        }

        public override Expression<Func<PowerHistory, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.PowerId == identityValues[0] &&
                    x.PowerSeqNumber == int.Parse(identityValues[1]);
        }
    }

}
