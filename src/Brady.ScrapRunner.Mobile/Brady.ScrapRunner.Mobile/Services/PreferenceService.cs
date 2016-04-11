using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class PreferenceService : IPreferenceService
    {
        private readonly IRepository<PreferenceModel> _preferenceRepository;

        public PreferenceService(
            IRepository<PreferenceModel> preferenceRepository
            )
        {
            _preferenceRepository = preferenceRepository;
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
