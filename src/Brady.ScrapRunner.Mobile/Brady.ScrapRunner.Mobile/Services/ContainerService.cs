using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class ContainerService : IContainerService
    {
        private readonly IConnectionService _connection;
        private readonly IRepository<ContainerMasterModel> _containerMasterRepository;

        public ContainerService(IConnectionService connection, IRepository<ContainerMasterModel> containerMasterRepository)
        {
            _connection = connection;
            _containerMasterRepository = containerMasterRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerMaster"></param>
        /// <returns></returns>
        public Task UpdateContainerMaster(List<ContainerMaster> containerMaster)
        {
            var mapped = AutoMapper.Mapper.Map<List<ContainerMaster>, List<ContainerMasterModel>>(containerMaster);
            return _containerMasterRepository.UpdateRangeAsync(mapped);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerChange"></param>
        /// <returns></returns>
        public Task UpdateContainerChangeIntoMaster(List<ContainerChange> containerChange)
        {
            var deleted = containerChange.FindAll(ct => ct.ActionFlag == ContainerChangeConstants.Delete).ToList();
            var nonDeleted = containerChange.FindAll(ct => ct.ActionFlag != ContainerChangeConstants.Delete).ToList();

            var deletedMapped = AutoMapper.Mapper.Map<List<ContainerChange>, List<ContainerMasterModel>>(deleted);
            foreach (var deletedItem in deletedMapped)
                _containerMasterRepository.DeleteAsync(deletedItem);

            var nonDeletedMapped = AutoMapper.Mapper.Map<List<ContainerChange>, List<ContainerMasterModel>>(nonDeleted);
            return _containerMasterRepository.InsertOrReplaceRangeAsync(nonDeletedMapped);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerNumber"></param>
        /// <returns></returns>
        public async Task<ContainerMasterModel> FindContainerAsync(string containerNumber)
        {
            return await _containerMasterRepository.FindAsync(ct => ct.ContainerNumber == containerNumber);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerId"></param>
        /// <returns></returns>
        public async Task<List<ContainerMasterModel>> FindPowerIdContainersAsync(string powerId)
        {
            var containers = await _containerMasterRepository.AsQueryable()
                .Where(ct => ct.ContainerPowerId == powerId).ToListAsync();

            return containers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerId"></param>
        /// <param name="containerNumber"></param>
        /// <returns></returns>
        public async Task<int> RemoveContainerFromPowerId(string powerId, string containerNumber)
        {
            var container = await FindContainerAsync(containerNumber);
            container.ContainerPowerId = null;
            return await _containerMasterRepository.UpdateAsync(container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public async Task<int> UpdateContainerAsync(ContainerMasterModel container)
        {
            return await _containerMasterRepository.UpdateAsync(container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public async Task<int> CreateContainerAsync(ContainerMasterModel container)
        {
            return await _containerMasterRepository.InsertAsync(container);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerActionProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverContainerActionProcess>> ProcessContainerActionAsync(
            DriverContainerActionProcess containerActionProcess)
        {
            var containers =
                await
                    _connection.GetConnection()
                        .UpdateAsync(containerActionProcess, requeryUpdated: false);
            return containers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerChangeProcess"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<ContainerChangeProcess>> ProcessContainerChangeAsync(
            ContainerChangeProcess containerChangeProcess)
        {
            var containers =
                await
                    _connection.GetConnection()
                        .UpdateAsync(containerChangeProcess, requeryUpdated: false);
            return containers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newContainer"></param>
        /// <returns></returns>
        public async Task<ChangeResultWithItem<DriverNewContainerProcess>> ProcessNewContainerAsync(
            DriverNewContainerProcess newContainer)
        {
            var container = await _connection.GetConnection().UpdateAsync(newContainer, requeryUpdated: false);
            return container;
        }
    }
}
