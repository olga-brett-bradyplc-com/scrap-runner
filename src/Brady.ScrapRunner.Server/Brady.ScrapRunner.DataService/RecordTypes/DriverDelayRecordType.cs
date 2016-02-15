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

    [CreateAction("DriverDelay")]
    [EditAction("DriverDelay")]
    [DeleteAction("DriverDelay")]
    public class DriverDelayRecordType :
        ChangeableRecordType<DriverDelay, string, DriverDelayValidator, DriverDelayDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverDelay, DriverDelay>();
        }

        public override DriverDelay GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            //int parsedDelaySeqNumber;
            //int.TryParse(identityValues[0], out parsedDelaySeqNumber); 

            return new DriverDelay
            {
                DelaySeqNumber = int.Parse(identityValues[0]),
                DriverId = identityValues[1],
                TripNumber = identityValues[2]
            };
        }

        public override Expression<Func<DriverDelay, bool>> GetIdentityPredicate(DriverDelay item)
        {
            return x => x.DelaySeqNumber == item.DelaySeqNumber && 
                        x.DriverId == item.DriverId &&                      
                        x.TripNumber == item.TripNumber ;
        }

        public override Expression<Func<DriverDelay, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            //int parsedDelaySeqNumber;
            //int.TryParse(identityValues[0], out parsedDelaySeqNumber);

            return x => x.DelaySeqNumber == int.Parse(identityValues[0]) && 
                        x.DriverId == identityValues[1] &&
                        x.TripNumber == identityValues[2] ;
        }

    }
}