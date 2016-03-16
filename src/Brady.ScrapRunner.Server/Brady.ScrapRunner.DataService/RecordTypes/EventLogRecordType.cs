﻿using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

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
            Mapper.CreateMap<EventLog, EventLog>();
        }
    }
}