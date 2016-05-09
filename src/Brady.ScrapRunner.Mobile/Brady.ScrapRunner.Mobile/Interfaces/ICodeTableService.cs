using Brady.ScrapRunner.Domain.Models;


namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface ICodeTableService
    {
        Task<List<CodeTableModel>> FindCountryStatesAsync(string country);

        Task UpdateCodeTable(IEnumerable<CodeTable> codeTable);

    }
}