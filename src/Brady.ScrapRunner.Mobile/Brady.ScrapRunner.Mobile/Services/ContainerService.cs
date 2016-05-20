using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class ContainerService : IContainerService
    {
        private readonly IRepository<ContainerMasterModel> _containerRepository;

        public ContainerService(IRepository<ContainerMasterModel> containerRepository)
        {
            _containerRepository = containerRepository;
        } 

        public Task UpdateContainerMaster(List<ContainerMaster> containerMaster)
        {
            var mapped = AutoMapper.Mapper.Map<List<ContainerMaster>, List<ContainerMasterModel>>(containerMaster);
            return _containerRepository.InsertRangeAsync(mapped);
        }

        public async Task<ContainerMasterModel> FindContainerAsync(string containerNumber)
        {
            return await _containerRepository.FindAsync(ct => ct.ContainerNumber == containerNumber);
        }

        public async Task<IEnumerable<ContainerMasterModel>> FindPowerIdContainersAsync(string powerId)
        {
            var containers = await _containerRepository.AsQueryable()
                .Where(ct => ct.ContainerPowerId == powerId).ToListAsync();

            return containers;
        }

        public async Task<int> UpdateNbContainerAsync(string containerNumber)
        {
            var container = await FindContainerAsync(containerNumber);
            container.ContainerType = "LUGR";
            return await _containerRepository.UpdateAsync(container);
        }

        public async Task<int> RemoveContainerFromPowerId(string powerId, string containerNumber)
        {
            var container = await FindContainerAsync(containerNumber);
            container.ContainerPowerId = null;
            return await _containerRepository.UpdateAsync(container);
        }
    }
}
