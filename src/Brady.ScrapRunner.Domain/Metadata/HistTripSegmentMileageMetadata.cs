using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class HistTripSegmentMileageMetadata : TypeMetadataProvider<HistTripSegmentMileage>
    {
        public HistTripSegmentMileageMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            IntegerProperty(x => x.HistSeqNo)
              .IsId()
              .DisplayName("History Seq Number");

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber)
                .IsId()
                .DisplayName("Trip Seg Number");

            IntegerProperty(x => x.TripSegMileageSeqNumber)
                .IsId()
                .DisplayName("Trip Seg Mileage Seq Number");

            StringProperty(x => x.TripSegMileageState);
            StringProperty(x => x.TripSegMileageCountry);
            IntegerProperty(x => x.TripSegMileageOdometerStart);
            IntegerProperty(x => x.TripSegMileageOdometerEnd);
            StringProperty(x => x.TripSegLoadedFlag);
            StringProperty(x => x.TripSegMileagePowerId);
            StringProperty(x => x.TripSegMileageDriverId);
            StringProperty(x => x.TripSegMileageDriverName);

            ViewDefaults()
                .Property(x => x.HistSeqNo)
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.TripSegMileageSeqNumber)
                .Property(x => x.TripSegMileageState)
                .Property(x => x.TripSegMileageCountry)
                .Property(x => x.TripSegMileageOdometerStart)
                .Property(x => x.TripSegMileageOdometerEnd)
                .Property(x => x.TripSegLoadedFlag)
                .Property(x => x.TripSegMileagePowerId)
                .Property(x => x.TripSegMileageDriverId)
                .Property(x => x.TripSegMileageDriverName)

                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.HistSeqNo)
                .OrderBy(x => x.TripSegNumber)
                .OrderBy(x => x.TripSegMileageSeqNumber);

        }
    }

}
