using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface IMessagesService
    {
        Task<ChangeResultWithItem<DriverMessageProcess>> FindMsgsRemoteAsync(DriverMessageProcess driverMessageProcess);

        Task<MessagesModel> FindMessageAsync(int? msgId);

        Task<List<MessagesModel>> FindDrvrMsgsAsync(string employeeId);

        Task UpdateMessages(IEnumerable<Messages> messages);
    }
}