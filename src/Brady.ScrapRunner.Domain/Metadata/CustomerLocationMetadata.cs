using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CustomerLocationMetadata : TypeMetadataProvider<CustomerLocation>
    {
        public CustomerLocationMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CustHostCode)
                .IsId()
                .DisplayName("Cust Host Code");

            StringProperty(x => x.CustLocation)
                .IsId()
                .DisplayName("Cust Location");

            IntegerProperty(x => x.CustStandardMinutes);

            ViewDefaults()
                .Property(x => x.CustHostCode)
                .Property(x => x.CustLocation)
                .Property(x => x.CustStandardMinutes)

                .OrderBy(x => x.CustHostCode)
                .OrderBy(x => x.CustLocation);
        }
    }
}
