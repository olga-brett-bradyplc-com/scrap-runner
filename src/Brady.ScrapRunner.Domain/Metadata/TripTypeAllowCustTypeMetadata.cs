using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
 
    public class TripTypeAllowCustTypeMetadata : TypeMetadataProvider<TripTypeAllowCustType>
    {
        public TripTypeAllowCustTypeMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripTypeCode)
                .IsId()
                .DisplayName("Trip Type Code");

            IntegerProperty(x => x.TripTypeSeqNumber)
                .IsId()
                .DisplayName("Trip Type Seq Number");

            StringProperty(x => x.TripTypeCustType)
                .IsId()
                .DisplayName("Cust Type");

            ViewDefaults()
                .Property(x => x.TripTypeCode)
                .Property(x => x.TripTypeSeqNumber)
                .Property(x => x.TripTypeCustType)

                .OrderBy(x => x.TripTypeCode)
                .OrderBy(x => x.TripTypeSeqNumber)
                .OrderBy(x => x.TripTypeCustType);
        }
    }
}
