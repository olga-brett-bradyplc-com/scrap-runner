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

    [CreateAction("ErrorLog")]
    [EditAction("ErrorLog")]
    [DeleteAction("ErrorLog")]
    public class ErrorLogRecordType :
        ChangeableRecordType<ErrorLog, int, ErrorLogValidator, ErrorLogDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<ErrorLog, ErrorLog>();
        }

        public override ErrorLog GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new ErrorLog
            {
                ErrorId = int.Parse(identityValues[0])
            };
        }

        public override Expression<Func<ErrorLog, bool>> GetIdentityPredicate(ErrorLog item)
        {
            return x => x.ErrorId == item.ErrorId;
        }

        public override Expression<Func<ErrorLog, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.ErrorId == int.Parse(identityValues[0]);
        }

    }
}