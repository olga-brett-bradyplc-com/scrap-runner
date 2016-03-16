using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TerminalChangeMetadata : TypeMetadataProvider<TerminalChange>
    {
        public TerminalChangeMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TerminalId)
                .IsId()
                .DisplayName("Terminal Id");

            StringProperty(x => x.RegionId);
            StringProperty(x => x.CustType);
            StringProperty(x => x.CustHostCode);
            StringProperty(x => x.CustCode4_4);
            StringProperty(x => x.CustName);
            StringProperty(x => x.CustAddress1);
            StringProperty(x => x.CustAddress2);
            StringProperty(x => x.CustCity);
            StringProperty(x => x.CustState);
            StringProperty(x => x.CustZip);
            StringProperty(x => x.CustCountry);
            StringProperty(x => x.CustPhone1);
            StringProperty(x => x.CustContact1);
            DateProperty(x => x.CustOpenTime);
            DateProperty(x => x.CustCloseTime);
            IntegerProperty(x => x.CustLatitude);
            IntegerProperty(x => x.CustLongitude);
            IntegerProperty(x => x.CustRadius);
            DateProperty(x => x.ChgDateTime);
            StringProperty(x => x.ChgActionFlag);
            StringProperty(x => x.CustDriverInstructions);

            ViewDefaults()
                .Property(x => x.TerminalId)
                .Property(x => x.RegionId)
                .Property(x => x.CustType)
                .Property(x => x.CustHostCode)
                .Property(x => x.CustCode4_4)
                .Property(x => x.CustName)
                .Property(x => x.CustAddress1)
                .Property(x => x.CustAddress2)
                .Property(x => x.CustCity)
                .Property(x => x.CustState)
                .Property(x => x.CustZip)
                .Property(x => x.CustCountry)
                .Property(x => x.CustPhone1)
                .Property(x => x.CustContact1)
                .Property(x => x.CustOpenTime)
                .Property(x => x.CustCloseTime)
                .Property(x => x.CustLatitude)
                .Property(x => x.CustLongitude)
                .Property(x => x.CustRadius)
                .Property(x => x.ChgDateTime)
                .Property(x => x.ChgActionFlag)
                .Property(x => x.CustDriverInstructions)

                .OrderBy(x => x.TerminalId);
        }
    }
}
