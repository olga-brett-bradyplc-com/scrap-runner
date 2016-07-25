using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Messages = Domain.Models.Messages;

    public interface IMessagesService
    {
        Task UpdateMessages(IEnumerable<Messages> messages);

        Task<int> CreateMessageAsync(MessagesModel message);

        Task<int> UpdateMessageAsync(MessagesModel message);

        Task<int> UpsertMessageAsync(MessagesModel message);

        Task<MessagesModel> FindMessageAsync(int? msgId);

        Task<List<MessagesModel>> FindMessagesAsync(string senderId);

        Task<List<EmployeeMasterModel>> FindApprovedUsersForMessagingAsync();

        Task<EmployeeMasterModel> FindEmployeeAsync(string employeeId);

        Task UpdateApprovedUsersForMessaging(IEnumerable<EmployeeMaster> users);
        
        Task<ChangeResultWithItem<DriverMessageProcess>> ProcessDriverMessagesAsync(DriverMessageProcess driverMessageProcess);
    }
}