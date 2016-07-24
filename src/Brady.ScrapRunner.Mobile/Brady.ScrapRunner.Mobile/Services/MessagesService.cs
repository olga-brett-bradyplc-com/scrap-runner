using System.Threading;
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
    using Messages = Domain.Models.Messages;

    public class MessagesService : IMessagesService
    {
        private readonly IConnectionService _connection;
        private readonly IRepository<MessagesModel> _messagesRepository;
        private readonly IRepository<EmployeeMasterModel> _employeeRepository;
        private readonly IRepository<DriverStatusModel> _driverStatusRepository; 

        public MessagesService(IRepository<MessagesModel> messagesRepository, 
            IRepository<EmployeeMasterModel> employeeRepository,
            IRepository<DriverStatusModel> driverStatusRepository,
            IConnectionService connection)
        {
            _connection = connection;
            _messagesRepository = messagesRepository;
            _employeeRepository = employeeRepository;
            _driverStatusRepository = driverStatusRepository;
        }


        /// <summary>
        /// Update the Messages local db table with user messages via remote objs
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Task UpdateMessages(IEnumerable<Messages> messages)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<Messages>, IEnumerable<MessagesModel>>(messages);
            return _messagesRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Create a new, local message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<int> CreateMessageAsync(MessagesModel message)
        {
            return await _messagesRepository.InsertAsync(message);
        }

        /// <summary>
        /// Update a existing, local message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<int> UpdateMessageAsync(MessagesModel message)
        {
            return await _messagesRepository.UpdateAsync(message);
        }

        /// <summary>
        /// Inserts or updates an existing message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<int> UpsertMessageAsync(MessagesModel message)
        {
            return _messagesRepository.InsertOrReplaceAsync(message);
        }

        /// <summary>
        /// Find a specific message with the given msgId
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public async Task<MessagesModel> FindMessageAsync(int? msgId)
        {
            var message = await _messagesRepository.FindAsync(t => t.MsgId == msgId);

            var currentDriverId = await FindCurrentDriver();
            message.LocalUser = currentDriverId.EmployeeId;

            return message;
        }

        /// <summary>
        /// Find all messages for a given user id
        /// </summary>
        /// <param name="senderId"></param>
        /// <returns></returns>
        public async Task<List<MessagesModel>> FindMessagesAsync(string senderId)
        {
            var messages = await _messagesRepository.AsQueryable()
                .Where(t => t.ReceiverId == senderId || t.SenderId == senderId)
                .OrderBy(t => t.CreateDateTime)
                .ToListAsync();

            var currentDriverId = await FindCurrentDriver();
            foreach (var message in messages)
                message.LocalUser = currentDriverId.EmployeeId;

            return messages;
        }

        /// <summary>
        /// Find all employees who have been approved for messaging. This is information gathered from SignInViewModel
        /// </summary>
        /// <returns></returns>
        public async Task<List<EmployeeMasterModel>> FindApprovedUsersForMessagingAsync()
        {
            var currentDriverId = await FindCurrentDriver();
            var users =
                await
                    _employeeRepository.AsQueryable()
                        .Where(e => e.EmployeeId != currentDriverId.EmployeeId).ToListAsync();
            return users;
        }

        /// <summary>
        /// Find specific information about an employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<EmployeeMasterModel> FindEmployeeAsync(string employeeId)
        {
            var user = await _employeeRepository.FindAsync(e => e.EmployeeId == employeeId);
            return user;
        }

        /// <summary>
        /// Update the EmployeeMaster local db table with approved users for messaging via remote objs
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public Task UpdateApprovedUsersForMessaging(IEnumerable<EmployeeMaster> users)
        {
            var mapped = AutoMapper.Mapper.Map<IEnumerable<EmployeeMaster>, IEnumerable<EmployeeMasterModel>>(users);
            return _employeeRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Find messages for the given driver
        /// </summary>
        /// <param name="driverMessageProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverMessageProcess>> ProcessDriverMessagesAsync(DriverMessageProcess driverMessageProcess)
        {
            var msgsTable = await _connection.GetConnection(ConnectionType.Online).UpdateAsync(driverMessageProcess, requeryUpdated: false);
            return msgsTable;
        }

        private async Task<DriverStatusModel> FindCurrentDriver()
        {
            // This is of course assuming only the current driver will be listed in the DriverStatus table
            var currentDriverStatus = await _driverStatusRepository.AllAsync();
            return currentDriverStatus.FirstOrDefault();
        }
    }
}
