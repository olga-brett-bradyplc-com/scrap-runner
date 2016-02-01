using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CodeTableHdrMetadata : TypeMetadataProvider<CodeTableHdr>
    {
        public CodeTableHdrMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CodeName)
                .IsId()
                .DisplayName("Code Name");

            StringProperty(x => x.CodeDesc);
            StringProperty(x => x.CodeType);
            StringProperty(x => x.AppliesTo);

            ViewDefaults()
                .Property(x => x.CodeName)
                .Property(x => x.CodeDesc)
                .Property(x => x.CodeType)
                .Property(x => x.AppliesTo)

                .OrderBy(x => x.CodeName);
        }
    }
}
