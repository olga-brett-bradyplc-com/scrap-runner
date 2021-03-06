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

    [CreateAction("TripSegmentImage")]
    [EditAction("TripSegmentImage")]
    [DeleteAction("TripSegmentImage")]

    public class TripSegmentImageRecordType :
         ChangeableRecordType<TripSegmentImage, string, TripSegmentImageValidator, TripSegmentImageDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripSegmentImage, TripSegmentImage>();
        }

        public override Expression<Func<TripSegmentImage, bool>> GetIdentityPredicate(TripSegmentImage item)
        {
            return x => x.TripNumber == item.TripNumber &&
                        x.TripSegImageSeqId == item.TripSegImageSeqId &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<TripSegmentImage, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.TripNumber == identityValues[0] &&
                        x.TripSegImageSeqId == int.Parse(identityValues[1]) &&
                        x.TripSegNumber == identityValues[2];
        }
    }
}
