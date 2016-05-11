using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using BWF.DataServices.Metadata.Models;

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
        private readonly IRepository<MessagesModel> _messagesRepository;
        public MessagesService(IRepository<MessagesModel> messagesRepository)
        {
            _messagesRepository = messagesRepository;
        }
        /// <summary>
        /// Find messages for the given driver
        /// </summary>
        /// <param name="driverId"></param>
        /// <returns></returns>
        public async Task<List<MessagesModel>> FindDrvrMsgsAsync(string driverId)
        {
            var sortedMsgs = await _messagesRepository.AsQueryable()
                .Where(t => t.ReceiverId == driverId)
                .OrderBy(t => t.MsgId)
                .ToListAsync();
            return sortedMsgs;
        }

        public async Task<MessagesModel> FindMessageAsync(int? msgId)
        {
            var message = await _messagesRepository.FindAsync(t => t.MsgId == msgId);
            return message;
        }

        public Task UpdateMessages(IEnumerable<Messages> messages)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Messages>, IEnumerable<MessagesModel>>(messages);
            return _messagesRepository.InsertRangeAsync(mapped);
        }
    }
}
