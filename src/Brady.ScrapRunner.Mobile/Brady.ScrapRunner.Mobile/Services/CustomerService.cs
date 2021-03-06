﻿using System;
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
        private readonly IRepository<CustomerMasterModel> _customerMasterRepository;

        public CustomerService(
            IRepository<CustomerCommodityModel> customerCommodityRepository,
            IRepository<CustomerLocationModel> customerLocationRepository,
            IRepository<CustomerDirectionsModel> customerDirectionsRepository,
            IRepository<CustomerMasterModel> customerMasteRepository )
        {
            _customerDirectionsRepository = customerDirectionsRepository;
            _customerCommodityRepository = customerCommodityRepository;
            _customerLocationRepository = customerLocationRepository;
            _customerMasterRepository = customerMasteRepository;
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
            return _customerDirectionsRepository.InsertOrReplaceRangeAsync(mapped);
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
            return _customerLocationRepository.InsertOrReplaceRangeAsync(mapped);
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
            return _customerCommodityRepository.InsertOrReplaceRangeAsync(mapped);
        }

        /// <summary>
        /// Update the local DB with customer masters provided by the server
        /// </summary>
        /// <param name="customerMasters"></param>
        /// <returns></returns>
        public Task UpdateCustomerMaster(IEnumerable<CustomerMaster> customerMasters)
        {
            var mapped =
                AutoMapper.Mapper.Map<IEnumerable<CustomerMaster>, IEnumerable<CustomerMasterModel>>(
                    customerMasters);
            return _customerMasterRepository.InsertOrReplaceRangeAsync(mapped);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomerCommodityModel>> FindCustomerCommodites(string custHostCode)
        {
            var commodities = await _customerCommodityRepository.AsQueryable().Where(cc => cc.CustHostCode == custHostCode).ToListAsync();
            return commodities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomerLocationModel>> FindCustomerLocations(string custHostCode)
        {
            var locations = await _customerLocationRepository.AsQueryable().Where(ct => ct.CustHostCode == custHostCode).ToListAsync();
            return locations;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="custHostCode"></param>
        /// <returns></returns>
        public async Task<CustomerMasterModel> FindCustomerMaster(string custHostCode)
        {
            var customer = await _customerMasterRepository.FindAsync(cs => cs.CustHostCode == custHostCode);
            return customer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="custHostCode"></param>
        /// <returns></returns>
        public async Task<List<CustomerDirectionsModel>> FindCustomerDirections(string custHostCode)
        {
            var directions = await _customerDirectionsRepository.AsQueryable().Where(d => d.CustHostCode == custHostCode).ToListAsync();
            return directions;
        }
        /// <summary>
        /// Update the local DB with customer masters provided by the server
        /// </summary>
        /// <param name="custHostCode"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public async Task<int> UpdateCustomerGpsCoordinates(string custHostCode, int lat, int lon)
        {
            var customer = await FindCustomerMaster(custHostCode);

            customer.CustHostCode = custHostCode;
            customer.CustLatitude = lat;
            customer.CustLongitude = lon;

            return await _customerMasterRepository.InsertOrReplaceAsync(customer);
        }
    }
}
