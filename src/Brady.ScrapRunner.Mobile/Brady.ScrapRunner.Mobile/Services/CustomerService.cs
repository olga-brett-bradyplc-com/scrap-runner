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
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<CustomerCommodityModel> _customerCommodityRepository;
        private readonly IRepository<CustomerLocationModel> _customerLocationRepository;
        private readonly IRepository<CustomerDirectionsModel> _customerDirectionsRepository;

        public CustomerService(
            IRepository<CustomerCommodityModel> customerCommodityRepository,
            IRepository<CustomerLocationModel> customerLocationRepository,
            IRepository<CustomerDirectionsModel> customerDirectionsRepository)
        {
            _customerDirectionsRepository = customerDirectionsRepository;
            _customerCommodityRepository = customerCommodityRepository;
            _customerLocationRepository = customerLocationRepository;
        }

        /// <summary>
        /// Update the local DB with customer directions provided by the server
        /// </summary>
        /// <param name="customerDirections"></param>
        /// <returns></returns>
        public Task UpdateCustomerDirections(IEnumerable<CustomerDirections> customerDirections)
        {
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<CustomerDirections>, IEnumerable<CustomerDirectionsModel>>(
                    customerDirections);
            return _customerDirectionsRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Update the local DB with customer locations provided by the server
        /// </summary>
        /// <param name="customerLocations"></param>
        /// <returns></returns>
        public Task UpdateCustomerLocation(IEnumerable<CustomerLocation> customerLocations)
        {
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<CustomerLocation>, IEnumerable<CustomerLocationModel>>(
                    customerLocations);
            return _customerLocationRepository.InsertRangeAsync(mapped);
        }

        /// <summary>
        /// Update the local DB with customer commodities provided by the server
        /// </summary>
        /// <param name="customerCommodities"></param>
        /// <returns></returns>
        public Task UpdateCustomerCommodity(IEnumerable<CustomerCommodity> customerCommodities)
        {
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<CustomerCommodity>, IEnumerable<CustomerCommodityModel>>(
                    customerCommodities);
            return _customerCommodityRepository.InsertRangeAsync(mapped);
        }
    }
}
