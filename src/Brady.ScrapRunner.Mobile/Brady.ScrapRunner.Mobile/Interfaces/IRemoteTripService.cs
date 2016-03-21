using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IRemoteTripService
    {
        Task<bool> UpdateTripStatusAsync(string tripNumber, string status, string statusDesc);

        Task<bool> UpdateTripSegmentStatusAsync(string tripNumber, string tripSegNumber, string status,
            string statusDesc);
    }
}
