using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class PreferenceMetadata : TypeMetadataProvider<Preference>
    {
        public PreferenceMetadata()
        {
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TerminalId)
                .IsId()
                .DisplayName("Terminal Id");

            StringProperty(x => x.Parameter)
                .IsId();

            StringProperty(x => x.ParameterValue)
                .DisplayName("Parameter Value")
                .AbbreviatedName("Value");

            StringProperty(x => x.Description)
                .IsFreeFormat();

            ViewDefaults()
                .Property(x => x.TerminalId)
                .Property(x => x.Parameter)
                .Property(x => x.ParameterValue)
                .Property(x => x.Description)
                .OrderBy(x => x.TerminalId)
                .OrderBy(x => x.Parameter);

        }
    }
}