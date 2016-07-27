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

namespace Brady.ScrapRunner.Mobile.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly IConnectionService _connection;
        private readonly IRepository<TerminalMasterModel> _terminalMasterRepository;

        public TerminalService(
            IConnectionService connection,
            IRepository<TerminalMasterModel> terminalMasterRepository )
        {
            _connection = connection;
            _terminalMasterRepository = terminalMasterRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminalChanges"></param>
        /// <returns></returns>
        public Task UpdateTerminalChangeIntoMaster(List<TerminalChange> terminalChanges)
        {
            var mapped = AutoMapper.Mapper.Map<List<TerminalChange>, List<TerminalMasterModel>> (terminalChanges);
            return _terminalMasterRepository.InsertOrReplaceRangeAsync(mapped);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminalChangeProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<TerminalChangeProcess>> FindTerminalChangesRemoteAsync(TerminalChangeProcess terminalChangeProcess)
        {
            var terminalChanges =
                await
                    _connection.GetConnection(ConnectionType.Online)
                        .UpdateAsync(terminalChangeProcess, requeryUpdated: false);
            return terminalChanges;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        public async Task<TerminalMasterModel> FindTerminalMasterAsync(string terminalId)
        {
            var terminal = await _terminalMasterRepository.FindAsync(t => t.TerminalId == terminalId);
            return terminal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TerminalMasterModel>> FindAllTerminalsAsync()
        {
            return await _terminalMasterRepository.AllAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminalChanges"></param>
        /// <returns></returns>
        public Task<int> UpsertTerminalMasterAsync(IEnumerable<TerminalMasterModel> terminalChanges)
        {
            return _terminalMasterRepository.InsertOrReplaceRangeAsync(terminalChanges);
        }
    }
}