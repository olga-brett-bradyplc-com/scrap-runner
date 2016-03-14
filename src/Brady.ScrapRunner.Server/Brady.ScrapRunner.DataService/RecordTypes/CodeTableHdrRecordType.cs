using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("CodeTableHdr")]
    [EditAction("CodeTableHdr")]
    [DeleteAction("CodeTableHdr")]
    public class CodeTableHdrRecordType :
        ChangeableRecordType<CodeTableHdr, string, CodeTableHdrValidator, CodeTableHdrDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            Mapper.CreateMap<CodeTableHdr, CodeTableHdr>();
        }
  
        //
        // These identity methods only need to be implemented for COMPOSITE IDs.
        //

//        public override CodeTableHdr GetIdentityObject(string id)
//        {
//            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
//            return new CodeTableHdr
//            {
//                CodeName = identityValues[0]
//            };
//        }
//
//        public override Expression<Func<CodeTableHdr, bool>> GetIdentityPredicate(CodeTableHdr item)
//        {
//            return x => x.CodeName == item.CodeName;
//        }
//
//        public override Expression<Func<CodeTableHdr, bool>> GetIdentityPredicate(string id)
//        {
//            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
//            return x => x.CodeName == identityValues[0];
//        }

    }
}