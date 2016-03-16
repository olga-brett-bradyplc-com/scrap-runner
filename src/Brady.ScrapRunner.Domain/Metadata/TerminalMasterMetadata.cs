using System;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TerminalMasterMetadata : TypeMetadataProvider<TerminalMaster>
    {
        public TerminalMasterMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TerminalId)
                .IsId()
                .DisplayName("Terminal Id");
          
            StringProperty(x => x.Region);
            StringProperty(x => x.TerminalName);
            StringProperty(x => x.Address1);
            StringProperty(x => x.Address2);
            StringProperty(x => x.City);
            StringProperty(x => x.State);
            StringProperty(x => x.Zip);
            StringProperty(x => x.Country);
            StringProperty(x => x.Phone);
            IntegerProperty(x => x.EtakLatitude);
            IntegerProperty(x => x.EtakLongitude);
            IntegerProperty(x => x.Latitude);
            IntegerProperty(x => x.Longitude);
            StringProperty(x => x.DispatchZone);
            StringProperty(x => x.Geocoded);
            IntegerProperty(x => x.RegionIndex);
            IntegerProperty(x => x.TerminalIdNumber);
            StringProperty(x => x.MasterTerminal);
            TimeProperty(x => x.ChgDateTime);
            StringProperty(x => x.ChgEmployeeId);
            StringProperty(x => x.TerminalType);
            IntegerProperty(x => x.TimeZoneFactor);
            StringProperty(x => x.DaylightSavings);
            StringProperty(x => x.TerminalIdHost);
            StringProperty(x => x.FileNameHost);

            ViewDefaults()
                .Property(x => x.TerminalId)
                .Property(x => x.Region)
                .Property(x => x.TerminalName)
                .Property(x => x.Address1)
                .Property(x => x.Address2)
                .Property(x => x.City)
                .Property(x => x.State)
                .Property(x => x.Zip)
                .Property(x => x.Country)
                .Property(x => x.Phone)
                .Property(x => x.EtakLatitude)
                .Property(x => x.EtakLongitude)
                .Property(x => x.Latitude)
                .Property(x => x.Longitude)
                .Property(x => x.DispatchZone)
                .Property(x => x.Geocoded)
                .Property(x => x.RegionIndex)
                .Property(x => x.TerminalIdNumber)
                .Property(x => x.MasterTerminal)
                .Property(x => x.ChgDateTime)
                .Property(x => x.ChgEmployeeId)
                .Property(x => x.TerminalType)
                .Property(x => x.TimeZoneFactor)
                .Property(x => x.DaylightSavings)
                .Property(x => x.TerminalIdHost)
                .Property(x => x.FileNameHost)
                .OrderBy(x => x.TerminalId);
        }
    }
}
