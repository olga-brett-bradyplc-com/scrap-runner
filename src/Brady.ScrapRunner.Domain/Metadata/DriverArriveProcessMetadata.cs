﻿using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverArriveProcessMetadata : TypeMetadataProvider<DriverArriveProcess>
    {
        public DriverArriveProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.TripNumber);
            StringProperty(x => x.TripSegNumber);
            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.PowerId);
            IntegerProperty(x => x.Odometer);
            StringProperty(x => x.GPSAutoFlag);
            IntegerProperty(x => x.Latitude);
            IntegerProperty(x => x.Longitude);
            StringProperty(x => x.Mdtid);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.TripNumber)
            .Property(x => x.TripSegNumber)
            .Property(x => x.ActionDateTime)
            .Property(x => x.PowerId)
            .Property(x => x.Odometer)
            .Property(x => x.GPSAutoFlag)
            .Property(x => x.Latitude)
            .Property(x => x.Longitude)
            .Property(x => x.Mdtid)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
