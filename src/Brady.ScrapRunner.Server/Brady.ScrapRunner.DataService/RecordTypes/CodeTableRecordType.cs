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

    [CreateAction("CodeTable")]
    [EditAction("CodeTable")]
    [DeleteAction("CodeTable")]
    public class CodeTableRecordType :
        ChangeableRecordType<CodeTable, long, string, CodeTableValidator, CodeTableDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CodeTable, CodeTable>();
        }

        public override CodeTable GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CodeTable
            {
                CodeName = identityValues[0],
                CodeValue = identityValues[1]
            };
        }

        public override Expression<Func<CodeTable, bool>> GetIdentityPredicate(CodeTable item)
        {
            return x => x.CodeName == item.CodeName &&
                        x.CodeValue == item.CodeValue;
        }

        public override Expression<Func<CodeTable, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CodeName == identityValues[0] &&
                        x.CodeValue == identityValues[1];
        }

    }
}