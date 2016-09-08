﻿using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class EmployeePreferencesMetadata : TypeMetadataProvider<EmployeePreferences>
    {
        public EmployeePreferencesMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.RegionId)
                .IsId()
                .DisplayName("Region Id");

            StringProperty(x => x.TerminalId)
                .IsId()
                .DisplayName("Terminal Id");

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.Parameter)
               .IsId()
               .DisplayName("Parameter");

            StringProperty(x => x.ParameterValue);
            StringProperty(x => x.Description);
            StringProperty(x => x.PreferenceType);
            IntegerProperty(x => x.PreferenceSeqNo);

            ViewDefaults()
                .Property(x => x.RegionId)
                .Property(x => x.TerminalId)
                .Property(x => x.EmployeeId)
                .Property(x => x.Parameter)
                .Property(x => x.ParameterValue)
                .Property(x => x.Description)
                .Property(x => x.PreferenceType)
                .Property(x => x.PreferenceSeqNo)

                .OrderBy(x => x.RegionId)
                .OrderBy(x => x.TerminalId)
                .OrderBy(x => x.EmployeeId)
                .OrderBy(x => x.PreferenceType)
                .OrderBy(x => x.PreferenceSeqNo);

        }
    }
}
