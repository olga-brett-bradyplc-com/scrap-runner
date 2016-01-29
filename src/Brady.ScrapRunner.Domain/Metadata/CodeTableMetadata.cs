using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CodeTableMetadata : TypeMetadataProvider<CodeTable>
    {
        public CodeTableMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CodeName)
                .IsId()
                .DisplayName("Code Name");

            StringProperty(x => x.CodeValue)
                .IsId()
                .DisplayName("Code Value");

            IntegerProperty(x => x.CodeSeq);
            StringProperty(x => x.CodeDisp1);
            StringProperty(x => x.CodeDisp2);
            StringProperty(x => x.CodeDisp3);
            StringProperty(x => x.CodeDisp4);
            StringProperty(x => x.CodeDisp5);
            StringProperty(x => x.CodeDisp6);

            ViewDefaults()
                .Property(x => x.CodeName)
                .Property(x => x.CodeValue)
                .Property(x => x.CodeSeq)
                .Property(x => x.CodeDisp1)
                .Property(x => x.CodeDisp2)
                .Property(x => x.CodeDisp3)
                .Property(x => x.CodeDisp4)
                .Property(x => x.CodeDisp5)
                .Property(x => x.CodeDisp6)

                .OrderBy(x => x.CodeName)
                .OrderBy(x => x.CodeValue);
        }
    }
}
