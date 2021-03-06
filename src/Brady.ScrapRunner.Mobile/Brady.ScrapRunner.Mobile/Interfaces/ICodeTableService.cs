﻿using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Models;


namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface ICodeTableService
    {
        Task<ChangeResultWithItem<CodeTableProcess>> FindCodesRemoteAsync(CodeTableProcess codeTableProcess);

        Task<List<CodeTableModel>> FindCountryStatesAsync(string country);

        Task<List<CodeTableModel>> FindCodeTableList(string codeName);

        Task<CodeTableModel> FindCodeTableObject(string codeName, string codeValue);

        Task UpdateCodeTable(IEnumerable<CodeTable> codeTable);

    }
}