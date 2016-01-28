using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class PowerHistoryMetadata : TypeMetadataProvider<PowerHistory>
    {
        public PowerHistoryMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.PowerId)
                .IsId()
                .DisplayName("Power Id");

            IntegerProperty(x => x.PowerSeqNumber)
                .IsId()
                .DisplayName("Power Seq Number");

            StringProperty(x => x.PowerType);
            StringProperty(x => x.PowerDesc);
            StringProperty(x => x.PowerSize);
            IntegerProperty(x => x.PowerLength);
            IntegerProperty(x => x.PowerTareWeight);
            StringProperty(x => x.PowerCustType);
            StringProperty(x => x.PowerCustTypeDesc);
            StringProperty(x => x.PowerTerminalId);
            StringProperty(x => x.PowerTerminalName);
            StringProperty(x => x.PowerRegionId);
            StringProperty(x => x.PowerRegionName);
            StringProperty(x => x.PowerLocation);
            StringProperty(x => x.PowerStatus);
            DateProperty(x => x.PowerDateOutOfService);
            DateProperty(x => x.PowerDateInService);
            StringProperty(x => x.PowerDriverId);
            StringProperty(x => x.PowerDriverName);
            IntegerProperty(x => x.PowerOdometer);
            StringProperty(x => x.PowerComments);
            StringProperty(x => x.MdtId);
            StringProperty(x => x.PrimaryPowerType);
            StringProperty(x => x.PowerCustHostCode);
            StringProperty(x => x.PowerCustName);
            StringProperty(x => x.PowerCustAddress1);
            StringProperty(x => x.PowerCustAddress2);
            StringProperty(x => x.PowerCustCity);
            StringProperty(x => x.PowerCustState);
            StringProperty(x => x.PowerCustZip);
            StringProperty(x => x.PowerCustCountry);
            StringProperty(x => x.PowerCustCounty);
            StringProperty(x => x.PowerCustTownship);
            StringProperty(x => x.PowerCustPhone1);
            DateProperty(x => x.PowerLastActionDateTime);
            StringProperty(x => x.PowerStatusDesc);
            StringProperty(x => x.PowerCurrentTripNumber);
            StringProperty(x => x.PowerCurrentTripSegNumber);
            StringProperty(x => x.PowerCurrentTripSegType);
            StringProperty(x => x.PowerCurrentTripSegTypeDesc);

            ViewDefaults()
                .Property(x => x.PowerId)
                .Property(x => x.PowerSeqNumber)
                .Property(x => x.PowerType)
                .Property(x => x.PowerDesc)
                .Property(x => x.PowerSize)
                .Property(x => x.PowerLength)
                .Property(x => x.PowerTareWeight)
                .Property(x => x.PowerCustType)
                .Property(x => x.PowerCustTypeDesc)
                .Property(x => x.PowerTerminalId)
                .Property(x => x.PowerTerminalName)
                .Property(x => x.PowerRegionId)
                .Property(x => x.PowerRegionName)
                .Property(x => x.PowerLocation)
                .Property(x => x.PowerStatus)
                .Property(x => x.PowerDateOutOfService)
                .Property(x => x.PowerDateInService)
                .Property(x => x.PowerDriverId)
                .Property(x => x.PowerDriverName)
                .Property(x => x.PowerOdometer)
                .Property(x => x.PowerComments)
                .Property(x => x.MdtId)
                .Property(x => x.PrimaryPowerType)
                .Property(x => x.PowerCustHostCode)
                .Property(x => x.PowerCustName)
                .Property(x => x.PowerCustAddress1)
                .Property(x => x.PowerCustAddress2)
                .Property(x => x.PowerCustCity)
                .Property(x => x.PowerCustState)
                .Property(x => x.PowerCustZip)
                .Property(x => x.PowerCustCountry)
                .Property(x => x.PowerCustCounty)
                .Property(x => x.PowerCustTownship)
                .Property(x => x.PowerCustPhone1)
                .Property(x => x.PowerLastActionDateTime)
                .Property(x => x.PowerStatusDesc)
                .Property(x => x.PowerCurrentTripNumber)
                .Property(x => x.PowerCurrentTripSegNumber)
                .Property(x => x.PowerCurrentTripSegType)
                .Property(x => x.PowerCurrentTripSegTypeDesc)

                .OrderBy(x => x.PowerId)
                .OrderBy(x => x.PowerSeqNumber);
        }
    }
}
