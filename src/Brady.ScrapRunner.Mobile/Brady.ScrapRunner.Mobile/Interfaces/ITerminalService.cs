using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface ITerminalService
    {
        Task<ChangeResultWithItem<TerminalChangeProcess>> FindTerminalChangesRemoteAsync(
            TerminalChangeProcess terminalChangeProcess);
        Task<List<TerminalChangeModel>> FindAllTerminalChanges();
        Task UpdateTerminalChange(IEnumerable<TerminalChange> terminalChanges);
    }
}
