using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class ContainerMasterMetadata : TypeMetadataProvider<ContainerMaster>
    {
        public ContainerMasterMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.ContainerNumber)
                .IsId()
                .DisplayName("Container Number");

            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            StringProperty(x => x.ContainerUnits);
            IntegerProperty(x => x.ContainerLength);
            IntegerProperty(x => x.ContainerWeightTare);
            StringProperty(x => x.ContainerShortTerm);
            StringProperty(x => x.ContainerCustHostCode);
            StringProperty(x => x.ContainerCustType);
            StringProperty(x => x.ContainerTerminalId);
            StringProperty(x => x.ContainerRegionId);
            StringProperty(x => x.ContainerLocation);
            TimeProperty(x => x.ContainerLastActionDateTime);
            TimeProperty(x => x.ContainerPendingMoveDateTime);
            StringProperty(x => x.ContainerCurrentTripNumber);
            StringProperty(x => x.ContainerCurrentTripSegNumber);
            StringProperty(x => x.ContainerCurrentTripSegType);
            StringProperty(x => x.ContainerContents);
            StringProperty(x => x.ContainerStatus);
            StringProperty(x => x.ContainerCommodityCode);
            StringProperty(x => x.ContainerCommodityDesc);
            StringProperty(x => x.ContainerComments);
            StringProperty(x => x.ContainerPowerId);
            StringProperty(x => x.ContainerBarCodeFlag);
            TimeProperty(x => x.ContainerAddDateTime);
            StringProperty(x => x.ContainerAddUserId);
            StringProperty(x => x.ContainerRestrictToHostCode);
            StringProperty(x => x.ContainerPrevCustHostCode);
            StringProperty(x => x.ContainerPrevTripNumber);
            IntegerProperty(x => x.ContainerLevel);
            IntegerProperty(x => x.ContainerLatitude);
            IntegerProperty(x => x.ContainerLongitude);
            StringProperty(x => x.ContainerNotes);
            StringProperty(x => x.ContainerCurrentTerminalId);
            StringProperty(x => x.ContainerManufacturer);
            NumericProperty(x => x.ContainerCost);
            StringProperty(x => x.ContainerSerialNumber);
            StringProperty(x => x.ContainerOrigin);
            DateProperty(x => x.ContainerPurchaseDate);
            StringProperty(x => x.LocationWarningFlag);
            IntegerProperty(x => x.ContainerWidth);
            IntegerProperty(x => x.ContainerHeight);
            StringProperty(x => x.ContainerBarCodeNo);
            StringProperty(x => x.ContainerInboundTerminalId);
            StringProperty(x => x.ContainerQtyInIDFlag);

            ViewDefaults()
                .Property(x => x.ContainerNumber)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.ContainerUnits)
                .Property(x => x.ContainerLength)
                .Property(x => x.ContainerWeightTare)
                .Property(x => x.ContainerShortTerm)
                .Property(x => x.ContainerCustHostCode)
                .Property(x => x.ContainerCustType)
                .Property(x => x.ContainerTerminalId)
                .Property(x => x.ContainerRegionId)
                .Property(x => x.ContainerLocation)
                .Property(x => x.ContainerLastActionDateTime)
                .Property(x => x.ContainerPendingMoveDateTime)
                .Property(x => x.ContainerCurrentTripNumber)
                .Property(x => x.ContainerCurrentTripSegNumber)
                .Property(x => x.ContainerCurrentTripSegType)
                .Property(x => x.ContainerContents)
                .Property(x => x.ContainerStatus)
                .Property(x => x.ContainerCommodityCode)
                .Property(x => x.ContainerCommodityDesc)
                .Property(x => x.ContainerComments)
                .Property(x => x.ContainerPowerId)
                .Property(x => x.ContainerBarCodeFlag)
                .Property(x => x.ContainerAddDateTime)
                .Property(x => x.ContainerAddUserId)
                .Property(x => x.ContainerRestrictToHostCode)
                .Property(x => x.ContainerPrevCustHostCode)
                .Property(x => x.ContainerPrevTripNumber)
                .Property(x => x.ContainerLevel)
                .Property(x => x.ContainerLatitude)
                .Property(x => x.ContainerLongitude)
                .Property(x => x.ContainerNotes)
                .Property(x => x.ContainerCurrentTerminalId)
                .Property(x => x.ContainerManufacturer)
                .Property(x => x.ContainerCost)
                .Property(x => x.ContainerSerialNumber)
                .Property(x => x.ContainerOrigin)
                .Property(x => x.ContainerPurchaseDate)
                .Property(x => x.LocationWarningFlag)
                .Property(x => x.ContainerWidth)
                .Property(x => x.ContainerHeight)
                .Property(x => x.ContainerBarCodeNo)
                .Property(x => x.ContainerInboundTerminalId)
                .Property(x => x.ContainerQtyInIDFlag)

                .OrderBy(x => x.ContainerNumber);
        }
    }
}
