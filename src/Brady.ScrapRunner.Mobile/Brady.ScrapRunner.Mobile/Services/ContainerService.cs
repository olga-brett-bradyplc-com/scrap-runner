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
        private readonly IConnectionService<DataServiceClient> _connection;  

        public ContainerService(IRepository<ContainerMasterModel> containerRepository, IConnectionService<DataServiceClient> connection )
        {
            _containerRepository = containerRepository;
            _connection = connection;
        } 

        public Task UpdateContainerMaster(List<ContainerMaster> containerMaster)
        {
            var mapped = AutoMapper.Mapper.Map<List<ContainerMaster>, List<ContainerMasterModel>>(containerMaster);
            return _containerRepository.InsertRangeAsync(mapped);
        }

        public async Task<IEnumerable<ContainerMasterModel>> FindPowerIdContainers(string powerId)
        {
            var containers = await _containerRepository.AsQueryable()
                .Where(ct => ct.ContainerPowerId == powerId).ToListAsync();

            return containers;
        }
    }
}
