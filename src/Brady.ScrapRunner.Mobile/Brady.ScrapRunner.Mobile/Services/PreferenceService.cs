using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class PreferenceService : IPreferenceService
    {
        private readonly IConnectionService _connection; 
        private readonly IRepository<PreferenceModel> _preferenceRepository;

        public PreferenceService( 
            IRepository<PreferenceModel> preferenceRepository, 
            IConnectionService connection)
        {
            _connection = connection;
            _preferenceRepository = preferenceRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preferenceProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<PreferencesProcess>> FindPreferencesRemoteAsync(PreferencesProcess preferenceProcess)
        {
            var preferences = await _connection.GetConnection(ConnectionType.Online).UpdateAsync(preferenceProcess, requeryUpdated: false);
            return preferences;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="preferenceCode"></param>
        /// <returns></returns>
        public async Task<string> FindPreferenceValueAsync(string preferenceCode)
        {
            var currentPreference = await _preferenceRepository.FindAsync(p => p.Parameter == preferenceCode);
            return currentPreference?.ParameterValue;
        }

        /// <summary>
        /// Update the local DB with preferences provided by server
        /// </summary>
        /// <param name="preferences"></param>
        /// <returns></returns>
        public Task UpdatePreferences(IEnumerable<Preference> preferences)
        {
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<Preference>, IEnumerable<PreferenceModel>>(
                    preferences);
            return _preferenceRepository.InsertRangeAsync(mapped);
        }
    }
}
