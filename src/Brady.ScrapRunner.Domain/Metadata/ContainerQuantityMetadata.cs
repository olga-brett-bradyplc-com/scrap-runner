using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
  
    public class ContainerQuantityMetadata : TypeMetadataProvider<ContainerQuantity>
    {
        public ContainerQuantityMetadata()
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

            StringProperty(x => x.CustTerminalId);
            StringProperty(x => x.ContainerType);
            StringProperty(x => x.ContainerSize);
            DateProperty(x => x.LastActionDateTime);
            StringProperty(x => x.LastTripNumber);
            StringProperty(x => x.LastTripSegNumber);
            StringProperty(x => x.LastTripSegType);
            IntegerProperty(x => x.LastQuantity);
            IntegerProperty(x => x.CurrentQuantity);
            DateProperty(x => x.ChangedDateTime);
            StringProperty(x => x.ChangedUserId);
            StringProperty(x => x.ChangedUserName);
            StringProperty(x => x.RemoveFromList);

            ViewDefaults()
                .Property(x => x.CustHostCode)
                .Property(x => x.CustSeqNo)
                .Property(x => x.CustTerminalId)
                .Property(x => x.ContainerType)
                .Property(x => x.ContainerSize)
                .Property(x => x.LastActionDateTime)
                .Property(x => x.LastTripNumber)
                .Property(x => x.LastTripSegNumber)
                .Property(x => x.LastTripSegType)
                .Property(x => x.LastQuantity)
                .Property(x => x.CurrentQuantity)
                .Property(x => x.ChangedDateTime)
                .Property(x => x.ChangedUserId)
                .Property(x => x.ChangedUserName)
                .Property(x => x.RemoveFromList)

                .OrderBy(x => x.CustHostCode)
                .OrderBy(x => x.CustSeqNo);
        }
    }
}
