using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
 
    public class TripSegmentImageMetadata : TypeMetadataProvider<TripSegmentImage>
    {
        public TripSegmentImageMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber)
                .IsId()
                .DisplayName("Trip Seg Number");

            IntegerProperty(x => x.TripSegImageSeqId)
                .IsId()
                .DisplayName("Trip Seg Image Seq Number");

            DateProperty(x => x.TripSegImageActionDateTime);
            StringProperty(x => x.TripSegImageLocation);
            StringProperty(x => x.TripSegImagePrintedName);
            StringProperty(x => x.TripSegImageType);

            ViewDefaults()
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.TripSegImageSeqId)
                .Property(x => x.TripSegImageActionDateTime)
                .Property(x => x.TripSegImageLocation)
                .Property(x => x.TripSegImagePrintedName)
                .Property(x => x.TripSegImageType)
 
                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.TripSegNumber)
                .OrderBy(x => x.TripSegImageSeqId);
        }
    }
}
