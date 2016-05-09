using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IPreferenceService
    {
        Task<ChangeResultWithItem<PreferencesProcess>> FindPreferencesRemoteAsync(PreferencesProcess preferenceProcess);

        Task<string> FindPreferenceValueAsync(string preferenceCode);

        Task UpdatePreferences(IEnumerable<Preference> preferences);
    }
}
