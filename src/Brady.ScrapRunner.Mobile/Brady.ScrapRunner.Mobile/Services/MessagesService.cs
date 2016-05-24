using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;

    public class MessagesService : IMessagesService
    {
        private readonly IConnectionService _connection;
        private readonly IRepository<MessagesModel> _messagesRepository;

        public MessagesService(IRepository<MessagesModel> messagesRepository, IConnectionService connection)
        {
            _connection = connection;
            _messagesRepository = messagesRepository;
        }
        /// <summary>
        /// Find messages for the given driver
        /// </summary>
        /// <param name="driverMessageProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverMessageProcess>> FindMsgsRemoteAsync(DriverMessageProcess driverMessageProcess)
        {
            var msgsTable = await _connection.GetConnection().UpdateAsync(driverMessageProcess, requeryUpdated: false);
            return msgsTable;
        }

        public async Task<MessagesModel> FindMessageAsync(int? msgId)
        {
            var message = await _messagesRepository.FindAsync(t => t.MsgId == msgId);
            return message;
        }

        public async Task<List<MessagesModel>> FindMsgsFromAsync(string senderId)
        {

            var paddedSender = senderId.PadRight(10, ' ');
            var message = await _messagesRepository.AsQueryable()
                .Where(t => t.ReceiverId == paddedSender || t.SenderId == paddedSender )
                .OrderBy(t => t.CreateDateTime)
                .ToListAsync();
            return message;
        }
        /// <summary>
        /// Find all messages for the given driver id
        /// </summary>
        /// <returns></returns>
        public async Task<List<MessagesModel>> SortedDrvrMsgsAsync()
        {
            var sortedMsgs = await _messagesRepository.AsQueryable()
                .OrderBy(t => t.CreateDateTime)
                .ToListAsync();
            return sortedMsgs;
        }

        public Task UpdateMessages(IEnumerable<Messages> messages)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Messages>, IEnumerable<MessagesModel>>(messages);
            return _messagesRepository.InsertRangeAsync(mapped);
        }
    }
}
