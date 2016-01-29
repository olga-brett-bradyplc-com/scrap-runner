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

    [CreateAction("DriverHistory")]
    [EditAction("DriverHistory")]
    [DeleteAction("DriverHistory")]
    public class DriverHistoryRecordType :
        ChangeableRecordType<DriverHistory, long, string, DriverHistoryValidator, DriverHistoryDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverHistory, DriverHistory>();
        }

        public override DriverHistory GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            //int parsedDriverSeqNumber;
            //int.TryParse(identityValues[2], out parsedDelaySeqNumber); 

            return new DriverHistory
            {
                EmployeeId = identityValues[0],
                TripNumber = identityValues[1],
                DriverSeqNumber = int.Parse(identityValues[2]), 
            };
        }

        public override Expression<Func<DriverHistory, bool>> GetIdentityPredicate(DriverHistory item)
        {
            return x => x.EmployeeId == item.EmployeeId &&
                        x.TripNumber == item.TripNumber &&
                        x.DriverSeqNumber == item.DriverSeqNumber;
        }

        public override Expression<Func<DriverHistory, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            //int parsedDriverSeqNumber;
            //int.TryParse(identityValues[2], out parsedDelaySeqNumber); 

            return x => x.EmployeeId == identityValues[0] &&
                        x.TripNumber == identityValues[1] &&
                        x.DriverSeqNumber == int.Parse(identityValues[2]);
        }

    }
}