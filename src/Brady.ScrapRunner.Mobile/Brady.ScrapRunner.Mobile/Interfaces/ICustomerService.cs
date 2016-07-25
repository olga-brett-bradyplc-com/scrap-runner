using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface ICustomerService
    {
        Task UpdateCustomerDirections(IEnumerable<CustomerDirections> customerDirections);
        Task UpdateCustomerLocation(IEnumerable<CustomerLocation> customerLocations);
        Task UpdateCustomerCommodity(IEnumerable<CustomerCommodity> customerCommodities);
        Task UpdateCustomerMaster(IEnumerable<CustomerMaster> customerMasters);
        Task<List<CustomerCommodityModel>> FindCustomerCommodites(string custHostCode);
        Task<List<CustomerLocationModel>> FindCustomerLocations(string custHostCode);
        Task<CustomerMasterModel> FindCustomerMaster(string custHostCode);
        Task<List<CustomerDirectionsModel>> FindCustomerDirections(string custHostCode);
    }
}
