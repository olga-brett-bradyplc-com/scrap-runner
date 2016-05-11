using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface IMessagesService
    {
        Task<List<MessagesModel>> FindDrvrMsgsAsync(string driver);

        Task<MessagesModel> FindMessageAsync(int? msgId);

        Task UpdateMessages(IEnumerable<Messages> messages);
    }
}