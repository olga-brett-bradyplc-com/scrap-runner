using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class PreferencesDefaultMetadata : TypeMetadataProvider<PreferencesDefault>
    {
        public PreferencesDefaultMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.Parameter)
                .IsId()
                .DisplayName("Parameter");

            StringProperty(x => x.ParameterValue);
            StringProperty(x => x.Description);
            StringProperty(x => x.PreferenceType);
            IntegerProperty(x => x.PreferenceSeqNo);

            ViewDefaults()
                .Property(x => x.Parameter)
                .Property(x => x.ParameterValue)
                .Property(x => x.Description)
                .Property(x => x.PreferenceType)
                .Property(x => x.PreferenceSeqNo)
                .OrderBy(x => x.PreferenceType)
                .OrderBy(x => x.PreferenceSeqNo);

        }
    }
}