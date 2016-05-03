using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;

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
    }
}
