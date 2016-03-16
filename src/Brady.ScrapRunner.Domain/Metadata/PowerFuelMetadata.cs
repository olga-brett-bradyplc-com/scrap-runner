using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class PowerFuelMetadata : TypeMetadataProvider<PowerFuel>
    {
        public PowerFuelMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.PowerId)
                .IsId()
                .DisplayName("Power Id");

            IntegerProperty(x => x.PowerFuelSeqNumber)
                .IsId()
                .DisplayName("Power Fuel Seq Number");

            StringProperty(x => x.TripNumber)
              .IsId()
              .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber);
            StringProperty(x => x.TripTerminalId);
            StringProperty(x => x.TripRegionId);
            StringProperty(x => x.TripDriverId);
            StringProperty(x => x.TripDriverName);
            DateProperty(x => x.PowerDateOfFuel);
            StringProperty(x => x.PowerState);
            StringProperty(x => x.PowerCountry);
            IntegerProperty(x => x.PowerOdometer);
            NumericProperty(x => x.PowerGallons);

            ViewDefaults()
                .Property(x => x.PowerId)
                .Property(x => x.PowerFuelSeqNumber)
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.TripTerminalId)
                .Property(x => x.TripRegionId)
                .Property(x => x.TripDriverId)
                .Property(x => x.TripDriverName)
                .Property(x => x.PowerDateOfFuel)
                .Property(x => x.PowerState)
                .Property(x => x.PowerCountry)
                .Property(x => x.PowerOdometer)
                .Property(x => x.PowerGallons)

                .OrderBy(x => x.PowerId)
                .OrderBy(x => x.PowerFuelSeqNumber)
                .OrderBy(x => x.TripNumber);

        }
    }
}
