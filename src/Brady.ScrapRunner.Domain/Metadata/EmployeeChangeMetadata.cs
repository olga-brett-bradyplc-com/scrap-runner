using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
   
    public class EmployeeChangeMetadata : TypeMetadataProvider<EmployeeChange>
    {
        public EmployeeChangeMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.LoginId)
               .IsId()
               .DisplayName("Login Id");

            StringProperty(x => x.RegionId)
                .IsId()
                .DisplayName("Region Id");

            StringProperty(x => x.ActionFlag)
                .IsId()
                .DisplayName("Action Flag");

            StringProperty(x => x.Password)
                .IsId()
                .DisplayName("Password");

            DateProperty(x => x.ChangeDateTime);

            ViewDefaults()
                .Property(x => x.EmployeeId)
                .Property(x => x.LoginId)
                .Property(x => x.RegionId)
                .Property(x => x.ActionFlag)
                .Property(x => x.Password)
                .Property(x => x.ChangeDateTime)

                .OrderBy(x => x.EmployeeId)
                .OrderBy(x => x.LoginId)
                .OrderBy(x => x.RegionId)
                .OrderBy(x => x.ActionFlag)
                .OrderBy(x => x.Password);
        }
    }
}
