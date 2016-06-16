using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Models;
using BWF.DataServices.Metadata.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IContainerService
    {
        Task<ChangeResultWithItem<ContainerChangeProcess>> FindContainerChangesRemoteAsync(
            ContainerChangeProcess containerChangeProcess);
        Task UpdateContainerMaster(List<ContainerMaster> containerMaster);
        Task UpdateContainerChange(List<ContainerChange> containerChange);
        Task<IEnumerable<ContainerMasterModel>> FindPowerIdContainersAsync(string powerId);
        Task<ContainerMasterModel> FindContainerAsync(string containerNumber);
        Task<int> UpdateNbContainerAsync(string containerNumber);
        Task<int> RemoveContainerFromPowerId(string powerId, string containerNumber);
    }
}
