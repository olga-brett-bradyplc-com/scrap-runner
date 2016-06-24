using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IRepository<ContainerChangeModel> _containerChangeRepository; 

        public ContainerService(IConnectionService connection, IRepository<ContainerMasterModel> containerMasterRepository, IRepository<ContainerChangeModel> containerChangeRepository)
        {
            _connection = connection;
            _containerMasterRepository = containerMasterRepository;
            _containerChangeRepository = containerChangeRepository;
        }

        public Task UpdateContainerMaster(List<ContainerMaster> containerMaster)
        {
            var mapped = AutoMapper.Mapper.Map<List<ContainerMaster>, List<ContainerMasterModel>>(containerMaster);
            return _containerMasterRepository.InsertRangeAsync(mapped);
        }

        public Task UpdateContainerChange(List<ContainerChange> containerChange)
        {
            var mapped = AutoMapper.Mapper.Map<List<ContainerChange>, List<ContainerChangeModel>>(containerChange);
            return _containerChangeRepository.InsertRangeAsync(mapped);
        }

        public async Task<ContainerMasterModel> FindContainerAsync(string containerNumber)
        {
            return await _containerMasterRepository.FindAsync(ct => ct.ContainerNumber == containerNumber);
        }

        public async Task<IEnumerable<ContainerMasterModel>> FindPowerIdContainersAsync(string powerId)
        {
            var containers = await _containerMasterRepository.AsQueryable()
                .Where(ct => ct.ContainerPowerId == powerId).ToListAsync();

            return containers;
        }

        public async Task<int> UpdateNbContainerAsync(string containerNumber)
        {
            var container = await FindContainerAsync(containerNumber);
            container.ContainerType = "LUGR";
            return await _containerMasterRepository.UpdateAsync(container);
        }

        public async Task<int> RemoveContainerFromPowerId(string powerId, string containerNumber)
        {
            var container = await FindContainerAsync(containerNumber);
            container.ContainerPowerId = null;
            return await _containerMasterRepository.UpdateAsync(container);
        }

        public async Task<ChangeResultWithItem<ContainerChangeProcess>> ProcessContainerChangeAsync(
            ContainerChangeProcess containerChangeProcess)
        {
            var containers =
                await
                    _connection.GetConnection(ConnectionType.Online)
                        .UpdateAsync(containerChangeProcess, requeryUpdated: false);
            return containers;
        }

        public async Task<ChangeResultWithItem<DriverNewContainerProcess>> ProcessNewContainerAsync(
            DriverNewContainerProcess newContainer)
        {
            var container = await _connection.GetConnection().UpdateAsync(newContainer, requeryUpdated: false);
            return container;
        }
    }
}
