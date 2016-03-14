using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
      public class ContainerQuantityHistoryMetadata : TypeMetadataProvider<ContainerQuantityHistory>
    {
        public ContainerQuantityHistoryMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CustHostCode)
                 .IsId()
                 .DisplayName("Cust Host Code");

            IntegerProperty(x => x.CustSeqNo)
                .IsId()
                .DisplayName("Cust Seq Number");

            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerTypeDesc);
            StringProperty(x => x.ContainerSize);
            DateProperty(x => x.LastActionDateTime);
            StringProperty(x => x.LastTripNumber);
            StringProperty(x => x.LastTripSegNumber);
            StringProperty(x => x.LastTripSegType);
            StringProperty(x => x.LastTripSegTypeDesc);
            IntegerProperty(x => x.LastQuantity);
            IntegerProperty(x => x.CurrentQuantity);
            DateProperty(x => x.ChangedDateTime);
            StringProperty(x => x.ChangedUserId);
            StringProperty(x => x.ChangedUserName);
            StringProperty(x => x.CustTerminalId);
            StringProperty(x => x.CustTerminalName);
            StringProperty(x => x.CustRegionId);
            StringProperty(x => x.CustRegionName);
            StringProperty(x => x.CustType);
            StringProperty(x => x.CustTypeDesc);
            StringProperty(x => x.CustName);
            StringProperty(x => x.CustAddress1);
            StringProperty(x => x.CustAddress2);
            StringProperty(x => x.CustCity);
            StringProperty(x => x.CustState);
            StringProperty(x => x.CustZip);
            StringProperty(x => x.CustCountry);
            StringProperty(x => x.CustCounty);
            StringProperty(x => x.CustTownship);
            StringProperty(x => x.CustPhone1);

            ViewDefaults()
                .Property(x => x.CustHostCode)
                .Property(x => x.CustSeqNo)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerTypeDesc)
                .Property(x => x.ContainerSize)
                .Property(x => x.LastActionDateTime)
                .Property(x => x.LastTripNumber)
                .Property(x => x.LastTripSegNumber)
                .Property(x => x.LastTripSegType)
                .Property(x => x.LastTripSegTypeDesc)
                .Property(x => x.LastQuantity)
                .Property(x => x.CurrentQuantity)
                .Property(x => x.ChangedDateTime)
                .Property(x => x.ChangedUserId)
                .Property(x => x.ChangedUserName)
                .Property(x => x.CustTerminalId)
                .Property(x => x.CustTerminalName)
                .Property(x => x.CustRegionId)
                .Property(x => x.CustRegionName)
                .Property(x => x.CustType)
                .Property(x => x.CustTypeDesc)
                .Property(x => x.CustName)
                .Property(x => x.CustAddress1)
                .Property(x => x.CustAddress2)
                .Property(x => x.CustCity)
                .Property(x => x.CustState)
                .Property(x => x.CustZip)
                .Property(x => x.CustCountry)
                .Property(x => x.CustCounty)
                .Property(x => x.CustTownship)
                .Property(x => x.CustPhone1)

                .OrderBy(x => x.CustHostCode)
                .OrderBy(x => x.CustSeqNo);
        }
    }
}
