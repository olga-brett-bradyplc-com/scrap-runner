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
 
    [CreateAction("TripTypeBasicDetails")]
    [EditAction("TripTypeBasicDetails")]
    [DeleteAction("TripTypeBasicDetails")]
    public class TripTypeBasicDetailsRecordType :
        ChangeableRecordType<TripTypeBasicDetails, int, TripTypeBasicDetailsValidator, TripTypeBasicDetailsDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<TripTypeBasicDetails, TripTypeBasicDetails>();
        }
    }
}
