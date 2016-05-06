using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;

    public class CodeTableService : ICodeTableService
    {
        private readonly IRepository<CodeTableModel> _codeTableRepository;
        public CodeTableService(IRepository<CodeTableModel> codeTableRepository)
        {
            _codeTableRepository = codeTableRepository;
        }
        /// <summary>
        /// Find all states for the given country code name
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        public async Task<List<CodeTableModel>> FindCountryStatesAsync(string country)
        {
            var sortedStates = await _codeTableRepository.AsQueryable()
                .Where(t => t.CodeName == country)
                .OrderBy(t => t.CodeValue)
                .ToListAsync();
            return sortedStates;
        }
        public Task UpdateCodeTable(IEnumerable<CodeTable> codeTable)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<CodeTable>, IEnumerable<CodeTableModel>>(codeTable);
            return _codeTableRepository.InsertRangeAsync(mapped);
        }
    }
}
