﻿using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Mappings
{
    public class DriverStatusMap : ClassMapping<DriverStatus>
    {
        public DriverStatusMap()
        {
                        
            Table("DriverStatus");

            Id(x => x.EmployeeId, m =>
            {
                m.Generator(Generators.Assigned);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("EmployeeId");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripNumber);
            Property(x => x.TripSegNumber);
            Property(x => x.TripSegType);
            Property(x => x.TripAssignStatus);
            Property(x => x.TripStatus);
            Property(x => x.TripSegStatus);
            // Known as DriverStatus in the scraprunner database.
            Property(x => x.Status, m =>
                {
                    m.Column("DriverStatus");
                });
            Property(x => x.TerminalId);
            Property(x => x.RegionId);
            Property(x => x.PowerId);
            Property(x => x.DriverArea);
            Property(x => x.MDTId);
            Property(x => x.LoginDateTime);
            Property(x => x.ActionDateTime);
            Property(x => x.DriverCumMinutes);
            Property(x => x.Odometer);
            Property(x => x.RFIDFlag);
            Property(x => x.RouteTo);
            Property(x => x.LoginProcessedDateTime);
            Property(x => x.GPSAutoGeneratedFlag);
            Property(x => x.ContainerMasterDateTime);
            Property(x => x.DelayCode);
            Property(x => x.PrevDriverStatus);
            Property(x => x.MdtVersion);
            Property(x => x.GPSXmitFlag);
            Property(x => x.SendHHLogoffFlag);
            Property(x => x.TerminalMasterDateTime);
            Property(x => x.DriverLCID);
            Property(x => x.ServicesFlag);
        }
    }
}
