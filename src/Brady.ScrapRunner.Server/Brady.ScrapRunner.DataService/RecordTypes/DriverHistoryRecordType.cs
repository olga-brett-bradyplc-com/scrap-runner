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
        ChangeableRecordType<DriverHistory, string, DriverHistoryValidator, DriverHistoryDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<DriverHistory, DriverHistory>();
        }

        public override DriverHistory GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            //int parsedDriverSeqNumber;
            //int.TryParse(identityValues[0], out parsedDriverSeqNumber); 

            return new DriverHistory
            {
                DriverSeqNumber = int.Parse(identityValues[0]),
                EmployeeId = identityValues[1],
                TripNumber = identityValues[2]
            };
        }

        public override Expression<Func<DriverHistory, bool>> GetIdentityPredicate(DriverHistory item)
        {
            return x => x.DriverSeqNumber == item.DriverSeqNumber &&
                        x.EmployeeId == item.EmployeeId &&
                        x.TripNumber == item.TripNumber ;
        }

        public override Expression<Func<DriverHistory, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            //int parsedDriverSeqNumber;
            //int.TryParse(identityValues[0], out parsedDriverSeqNumber); 

            return x => x.DriverSeqNumber == int.Parse(identityValues[0]) &&
                        x.EmployeeId == identityValues[1] &&
                        x.TripNumber == identityValues[2] ;
        }

    }
}