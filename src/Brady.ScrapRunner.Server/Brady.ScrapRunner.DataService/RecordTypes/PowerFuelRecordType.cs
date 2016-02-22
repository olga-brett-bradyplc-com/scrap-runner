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
    [CreateAction("PowerFuel")]
    [EditAction("PowerFuel")]
    [DeleteAction("PowerFuel")]
    public class PowerFuelRecordType :
        ChangeableRecordType<PowerFuel, string, PowerFuelValidator, PowerFuelDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<PowerFuel, PowerFuel>();
        }

        public override PowerFuel GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new PowerFuel
            {
                PowerFuelSeqNumber = int.Parse(identityValues[0]),
                PowerId = identityValues[1],
                TripNumber = identityValues[2]
            };
        }

        public override Expression<Func<PowerFuel, bool>> GetIdentityPredicate(PowerFuel item)
        {
            return x => x.PowerFuelSeqNumber == item.PowerFuelSeqNumber &&
                        x.PowerId == item.PowerId &&
                        x.TripNumber == item.TripNumber;
        }

        public override Expression<Func<PowerFuel, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.PowerFuelSeqNumber == int.Parse(identityValues[0]) &&
                        x.PowerId == identityValues[1] &&
                        x.TripNumber == identityValues[2];
        }
    }

}
