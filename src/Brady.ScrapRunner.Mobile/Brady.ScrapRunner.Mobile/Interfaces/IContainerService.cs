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
        Task UpdateContainerMaster(List<ContainerMaster> containerMaster);

        Task UpdateContainerChangeIntoMaster(List<ContainerChange> containerChange);

        Task<List<ContainerMasterModel>> FindPowerIdContainersAsync(string powerId);

        Task<ContainerMasterModel> FindContainerAsync(string containerNumber);

        Task<int> UpdateContainerAsync(ContainerMasterModel container);

        Task<int> CreateContainerAsync(ContainerMasterModel container);

        Task<bool> HasMultipleContainersLoadedAsync(string powerId);

        Task<int> UnloadContainerFromPowerIdAsync(string powerId, string containerNumber);

        Task<int> ResetContainer(ContainerMasterModel container, bool forceUnload = false);

        Task<int> LoadContainerOnPowerIdAsync(string powerId, string containerNumber, string custHostCode = null, TripSegmentContainerModel tripContainer = null);

        Task<ChangeResultWithItem<DriverContainerActionProcess>> ProcessContainerActionAsync(
            DriverContainerActionProcess containerActionProcess);

        Task<ChangeResultWithItem<ContainerChangeProcess>> ProcessContainerChangeAsync(
            ContainerChangeProcess containerChangeProcess);

        Task<ChangeResultWithItem<DriverNewContainerProcess>> ProcessNewContainerAsync(
            DriverNewContainerProcess newContainer);
    }
}
