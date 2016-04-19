using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class ContainerHistoryMetadata : TypeMetadataProvider<ContainerHistory>
    {
        public ContainerHistoryMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.ContainerNumber)
                .IsId()
                .DisplayName("Container Number");

            IntegerProperty(x => x.ContainerSeqNumber)
               .IsId()
               .DisplayName("Container Seq Number");

            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            StringProperty(x => x.ContainerUnits);
            IntegerProperty(x => x.ContainerLength);
            StringProperty(x => x.ContainerCustHostCode);
            StringProperty(x => x.ContainerCustType);
            StringProperty(x => x.ContainerCustTypeDesc);
            StringProperty(x => x.ContainerTerminalId);
            StringProperty(x => x.ContainerTerminalName);
            StringProperty(x => x.ContainerRegionId);
            StringProperty(x => x.ContainerRegionName);
            StringProperty(x => x.ContainerLocation);
            TimeProperty(x => x.ContainerLastActionDateTime);
            IntegerProperty(x => x.ContainerDaysAtSite);
            TimeProperty(x => x.ContainerPendingMoveDateTime);
            StringProperty(x => x.ContainerTripNumber);
            StringProperty(x => x.ContainerTripSegNumber);
            StringProperty(x => x.ContainerTripSegType);
            StringProperty(x => x.ContainerTripSegTypeDesc);
            StringProperty(x => x.ContainerContents);
            StringProperty(x => x.ContainerContentsDesc);
            StringProperty(x => x.ContainerStatus);
            StringProperty(x => x.ContainerStatusDesc);
            StringProperty(x => x.ContainerCommodityCode);
            StringProperty(x => x.ContainerCommodityDesc);
            StringProperty(x => x.ContainerComments);
            StringProperty(x => x.ContainerPowerId);
            StringProperty(x => x.ContainerShortTerm);
            StringProperty(x => x.ContainerCustName);
            StringProperty(x => x.ContainerCustAddress1);
            StringProperty(x => x.ContainerCustAddress2);
            StringProperty(x => x.ContainerCustCity);
            StringProperty(x => x.ContainerCustState);
            StringProperty(x => x.ContainerCustZip);
            StringProperty(x => x.ContainerCustCountry);
            StringProperty(x => x.ContainerCustCounty);
            StringProperty(x => x.ContainerCustTownship);
            StringProperty(x => x.ContainerCustPhone1);
            IntegerProperty(x => x.ContainerLevel);
            IntegerProperty(x => x.ContainerLatitude);
            IntegerProperty(x => x.ContainerLongitude);
            StringProperty(x => x.ContainerNotes);
            StringProperty(x => x.ContainerCurrentTerminalId);
            StringProperty(x => x.ContainerCurrentTerminalName);
            IntegerProperty(x => x.ContainerWidth);
            IntegerProperty(x => x.ContainerHeight);

            ViewDefaults()
                .Property(x => x.ContainerNumber)
                .Property(x => x.ContainerSeqNumber)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.ContainerUnits)
                .Property(x => x.ContainerLength)
                .Property(x => x.ContainerCustHostCode)
                .Property(x => x.ContainerCustType)
                .Property(x => x.ContainerCustTypeDesc)
                .Property(x => x.ContainerTerminalId)
                .Property(x => x.ContainerTerminalName)
                .Property(x => x.ContainerRegionId)
                .Property(x => x.ContainerRegionName)
                .Property(x => x.ContainerLocation)
                .Property(x => x.ContainerLastActionDateTime)
                .Property(x => x.ContainerDaysAtSite)
                .Property(x => x.ContainerPendingMoveDateTime)
                .Property(x => x.ContainerTripNumber)
                .Property(x => x.ContainerTripSegNumber)
                .Property(x => x.ContainerTripSegType)
                .Property(x => x.ContainerTripSegTypeDesc)
                .Property(x => x.ContainerContents)
                .Property(x => x.ContainerContentsDesc)
                .Property(x => x.ContainerStatus)
                .Property(x => x.ContainerStatusDesc)
                .Property(x => x.ContainerCommodityCode)
                .Property(x => x.ContainerCommodityDesc)
                .Property(x => x.ContainerComments)
                .Property(x => x.ContainerPowerId)
                .Property(x => x.ContainerShortTerm)
                .Property(x => x.ContainerCustName)
                .Property(x => x.ContainerCustAddress1)
                .Property(x => x.ContainerCustAddress2)
                .Property(x => x.ContainerCustCity)
                .Property(x => x.ContainerCustState)
                .Property(x => x.ContainerCustZip)
                .Property(x => x.ContainerCustCountry)
                .Property(x => x.ContainerCustCounty)
                .Property(x => x.ContainerCustTownship)
                .Property(x => x.ContainerCustPhone1)
                .Property(x => x.ContainerLevel)
                .Property(x => x.ContainerLatitude)
                .Property(x => x.ContainerLongitude)
                .Property(x => x.ContainerNotes)
                .Property(x => x.ContainerCurrentTerminalId)
                .Property(x => x.ContainerCurrentTerminalName)
                .Property(x => x.ContainerWidth)
                .Property(x => x.ContainerHeight)

                .OrderBy(x => x.ContainerNumber)
                .OrderBy(x => x.ContainerSeqNumber);
        }
    }
}
