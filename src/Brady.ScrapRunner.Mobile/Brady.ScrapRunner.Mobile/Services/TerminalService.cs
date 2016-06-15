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
        private readonly IRepository<TerminalChangeModel> _terminalChangeRepository;

        public TerminalService(IConnectionService connection, IRepository<TerminalChangeModel> terminalChangeRepository)
        {
            _connection = connection;
            _terminalChangeRepository = terminalChangeRepository;
        }
         
        public async Task<ChangeResultWithItem<TerminalChangeProcess>> FindTerminalChangesRemoteAsync(TerminalChangeProcess terminalChangeProcess)
        {
            var terminalChanges =
                await
                    _connection.GetConnection(ConnectionType.Online)
                        .UpdateAsync(terminalChangeProcess, requeryUpdated: false);
            return terminalChanges;
        }

        public async Task<List<TerminalChangeModel>> FindAllTerminalChanges()
        {
            return await _terminalChangeRepository.AllAsync();
        }

        public Task UpdateTerminalChange(IEnumerable<TerminalChange> terminalChanges)
        {
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<TerminalChange>, IEnumerable<TerminalChangeModel>>(terminalChanges);
            return _terminalChangeRepository.InsertRangeAsync(mapped);
        }
    }
}
