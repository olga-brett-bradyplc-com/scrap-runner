using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IContainerService
    {
        Task UpdateContainerMaster(List<ContainerMaster> containerMaster);
        Task<IEnumerable<ContainerMasterModel>> FindPowerIdContainersAsync(string powerId);
        Task<ContainerMasterModel> FindContainerAsync(string containerNumber);
        Task<int> UpdateNbContainerAsync(string containerNumber);
        Task<int> RemoveContainerFromPowerId(string powerId, string containerNumber);
    }
}
