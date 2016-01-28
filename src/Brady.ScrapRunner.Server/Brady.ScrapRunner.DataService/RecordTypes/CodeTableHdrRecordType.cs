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

    [CreateAction("CodeTableHdr")]
    [EditAction("CodeTableHdr")]
    [DeleteAction("CodeTableHdr")]
    public class CodeTableHdrRecordType :
        ChangeableRecordType<CodeTableHdr, long, string, CodeTableHdrValidator, CodeTableHdrDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<CodeTableHdr, CodeTableHdr>();
        }

        public override CodeTableHdr GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new CodeTableHdr
            {
                CodeName = identityValues[0]
            };
        }

        public override Expression<Func<CodeTableHdr, bool>> GetIdentityPredicate(CodeTableHdr item)
        {
            return x => x.CodeName == item.CodeName;
        }

        public override Expression<Func<CodeTableHdr, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.CodeName == identityValues[0];
        }

    }
}