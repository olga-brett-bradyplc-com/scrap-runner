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

    [CreateAction("EventLog")]
    [EditAction("EventLog")]
    [DeleteAction("EventLog")]
    public class EventLogRecordType :
        ChangeableRecordType<EventLog, int, EventLogValidator, EventLogDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<EventLog, EventLog>();
        }

        public override EventLog GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new EventLog
            {
                EventId = int.Parse(identityValues[0])
            };
        }

        public override Expression<Func<EventLog, bool>> GetIdentityPredicate(EventLog item)
        {
            return x => x.EventId == item.EventId;
        }

        public override Expression<Func<EventLog, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.EventId == int.Parse(identityValues[0]);
        }

    }
}