using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
  
    public class TripPointsMetadata : TypeMetadataProvider<TripPoints>
    {
        public TripPointsMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripPointsHostCode1)
              .IsId()
              .DisplayName("Trip Points HostCode1");

            StringProperty(x => x.TripPointsHostCode2)
                .IsId()
                .DisplayName("Trip Points HostCode2");

            IntegerProperty(x => x.TripPointsStandardMinutes);
            IntegerProperty(x => x.TripPointsStandardMiles);
            IntegerProperty(x => x.TripPointsSendToMaps);
            TimeProperty(x => x.ChgDateTime);
            StringProperty(x => x.ChgEmployeeId);


            ViewDefaults()
                .Property(x => x.TripPointsHostCode1)
                .Property(x => x.TripPointsHostCode2)
                .Property(x => x.TripPointsStandardMinutes)
                .Property(x => x.TripPointsStandardMiles)
                .Property(x => x.TripPointsSendToMaps)
                .Property(x => x.ChgDateTime)
                .Property(x => x.ChgEmployeeId)

                .OrderBy(x => x.TripPointsHostCode1)
                .OrderBy(x => x.TripPointsHostCode2);

        }
    }
}
