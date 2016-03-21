using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Interfaces;
using BWF.DataServices.PortableClients;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class RemoteTripService : IRemoteTripService
    {
        private readonly IConnectionService<DataServiceClient> _connection;

        public RemoteTripService(IConnectionService<DataServiceClient> connection)
        {
            _connection = connection;
        }

        public async Task<bool> UpdateTripStatusAsync(string tripNumber, string status, string statusDesc)
        {
            var trip = await _connection.GetConnection().GetAsync<string, Trip>(tripNumber);
            trip.TripStatus = status;
            trip.TripStatusDesc = statusDesc;
            var updateTrip = await _connection.GetConnection().UpdateAsync(trip);

            return updateTrip.WasSuccessful;
        }

        public async Task<bool> UpdateTripSegmentStatusAsync(string tripNumber, string tripSegNumber, string status,
            string statusDesc)
        {
            var tripSegment = await _connection.GetConnection().GetAsync<string, TripSegment>(tripNumber, tripSegNumber);
            tripSegment.TripSegStatus = status;
            tripSegment.TripSegStatusDesc = statusDesc;
            var updateTripSegment = await _connection.GetConnection().UpdateAsync(tripSegment);

            return updateTripSegment.WasSuccessful;
        }
    }
}