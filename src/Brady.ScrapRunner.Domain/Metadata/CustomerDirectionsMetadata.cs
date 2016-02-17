using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CustomerDirectionsMetadata : TypeMetadataProvider<CustomerDirections>
    {
        public CustomerDirectionsMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CustHostCode)
                 .IsId()
                 .DisplayName("Cust Host Code");

            IntegerProperty(x => x.DirectionsSeqNo)
                .IsId()
                .DisplayName("Dir Seq Number");

            StringProperty(x => x.DirectionsDesc);

            ViewDefaults()
                .Property(x => x.CustHostCode)
                .Property(x => x.DirectionsSeqNo)
                .Property(x => x.DirectionsDesc)

                .OrderBy(x => x.CustHostCode)
                .OrderBy(x => x.DirectionsSeqNo);
        }
    }
}
