using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{

    public class SecurityMasterMetadata : TypeMetadataProvider<SecurityMaster>
    {
        public SecurityMasterMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.SecurityLevel)
              .IsId()
              .DisplayName("Security Level");

            StringProperty(x => x.SecurityType)
                .IsId()
                .DisplayName("Security Type");

            StringProperty(x => x.SecurityProgram)
                .IsId()
                .DisplayName("Security Program");

            StringProperty(x => x.SecurityFunction)
                .IsId()
                .DisplayName("Security Function");

            StringProperty(x => x.SecurityDescription1);
            StringProperty(x => x.SecurityDescription2);
            StringProperty(x => x.SecurityDescription3);
            StringProperty(x => x.SecurityDescription4);
            IntegerProperty(x => x.SecurityAccess);

            ViewDefaults()
                .Property(x => x.SecurityLevel)
                .Property(x => x.SecurityType)
                .Property(x => x.SecurityProgram)
                .Property(x => x.SecurityFunction)
                .Property(x => x.SecurityDescription1)
                .Property(x => x.SecurityDescription2)
                .Property(x => x.SecurityDescription3)
                .Property(x => x.SecurityDescription4)
                .Property(x => x.SecurityAccess)

                .OrderBy(x => x.SecurityLevel)
                .OrderBy(x => x.SecurityType)
                .OrderBy(x => x.SecurityProgram)
                .OrderBy(x => x.SecurityFunction);

        }
    }
}
