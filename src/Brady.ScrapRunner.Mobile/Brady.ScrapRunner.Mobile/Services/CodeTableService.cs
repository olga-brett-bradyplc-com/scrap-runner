using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;

    public class CodeTableService : ICodeTableService
    {
        private readonly IConnectionService _connection; 
        private readonly IRepository<CodeTableModel> _codeTableRepository;

        public CodeTableService(IRepository<CodeTableModel> codeTableRepository, IConnectionService connection )
        {
            _connection = connection;
            _codeTableRepository = codeTableRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeTableProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<CodeTableProcess>> FindCodesRemoteAsync(CodeTableProcess codeTableProcess)
        {
            var codeTable = await _connection.GetConnection(ConnectionType.Online).UpdateAsync(codeTableProcess, requeryUpdated: false);
            return codeTable;
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

        /// <summary>
        /// Find a list of CodeTableModel objects based on CodeName column
        /// </summary>
        /// <param name="codeName"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CodeTableModel>> FindCodeTableList(string codeName)
        {
            var sortedCodes = await _codeTableRepository.AsQueryable()
                .Where(t => t.CodeName == codeName)
                .OrderBy(t => t.CodeValue)
                .ToListAsync();
            return sortedCodes;
        }

        /// <summary>
        /// Find a specific CodeTableModel object based on CodeName and CodeValue
        /// </summary>
        /// <param name="codeName"></param>
        /// <param name="codeValue"></param>
        /// <returns></returns>
        public async Task<CodeTableModel> FindCodeTableObject(string codeName, string codeValue)
        {
            var ct = await _codeTableRepository.FindAsync(v => v.CodeName == codeName && v.CodeValue == codeValue);
            return ct;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeTable"></param>
        /// <returns></returns>
        public Task UpdateCodeTable(IEnumerable<CodeTable> codeTable)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<CodeTable>, IEnumerable<CodeTableModel>>(codeTable);
            return _codeTableRepository.InsertRangeAsync(mapped);
        }
    }
}
