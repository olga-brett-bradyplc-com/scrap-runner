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
    [CreateAction("DriverEfficiency")]
    [EditAction("DriverEfficiency")]
    [DeleteAction("DriverEfficiency")]
    public class DriverEfficiencyRecordType :
         ChangeableRecordType<DriverEfficiency, string, DriverEfficiencyValidator, DriverEfficiencyDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverEfficiency, DriverEfficiency>();
        }

        public override DriverEfficiency GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new DriverEfficiency
            {
                TripDriverId = identityValues[0],
                TripNumber = identityValues[1]
            };
        }

        public override Expression<Func<DriverEfficiency, bool>> GetIdentityPredicate(DriverEfficiency item)
        {
            return x => x.TripDriverId == item.TripDriverId &&
                        x.TripNumber == item.TripNumber;
        }

        public override Expression<Func<DriverEfficiency, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripDriverId == identityValues[0] &&
                        x.TripNumber == identityValues[1];
        }
    }
}
