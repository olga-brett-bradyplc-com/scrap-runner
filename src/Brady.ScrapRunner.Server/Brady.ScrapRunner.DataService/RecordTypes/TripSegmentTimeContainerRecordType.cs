﻿using AutoMapper;
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
  
    [CreateAction("TripSegmentContainerTime")]
    [EditAction("TripSegmentContainerTime")]
    [DeleteAction("TripSegmentContainerTime")]
    public class TripSegmentTimeContainerRecordType :
        ChangeableRecordType<TripSegmentContainerTime, string, TripSegmentContainerTimeValidator, TripSegmentContainerTimeDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripSegmentContainerTime, TripSegmentContainerTime>();
        }

        public override Expression<Func<TripSegmentContainerTime, bool>> GetIdentityPredicate(TripSegmentContainerTime item)
        {
            return x => x.SeqNumber == item.SeqNumber &&
                        x.TripNumber == item.TripNumber &&
                        x.TripSegContainerSeqNumber == item.TripSegContainerSeqNumber &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<TripSegmentContainerTime, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.SeqNumber == int.Parse(identityValues[0]) &&
                        x.TripNumber == identityValues[1] &&
                        x.TripSegContainerSeqNumber == int.Parse(identityValues[2]) &&
                        x.TripSegNumber == identityValues[3];
        }
    }
}
