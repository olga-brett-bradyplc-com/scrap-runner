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
    [CreateAction("TripSegmentContainer")]
    [EditAction("TripSegmentContainer")]
    [DeleteAction("TripSegmentContainer")]
    public class TripSegmentContainerRecordType :
        ChangeableRecordType<TripSegmentContainer, string, TripSegmentContainerValidator, TripSegmentContainerDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripSegmentContainer, TripSegmentContainer>();
        }

        public override TripSegmentContainer GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new TripSegmentContainer
            {
                TripNumber = identityValues[0],
                TripSegContainerSeqNumber = int.Parse(identityValues[1]),
                TripSegNumber = identityValues[2]
            };
 
        }

        public override Expression<Func<TripSegmentContainer, bool >> GetIdentityPredicate(TripSegmentContainer item)
        {
            return x => x.TripNumber == item.TripNumber &&
                        x.TripSegContainerSeqNumber == item.TripSegContainerSeqNumber &&
                        x.TripSegNumber == item.TripSegNumber;
        }

        public override Expression<Func<TripSegmentContainer, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);

            return x => x.TripNumber == identityValues[0] &&
                        x.TripSegContainerSeqNumber == int.Parse(identityValues[1]) &&
            x.TripSegNumber == identityValues[2];

        }
    }
}
