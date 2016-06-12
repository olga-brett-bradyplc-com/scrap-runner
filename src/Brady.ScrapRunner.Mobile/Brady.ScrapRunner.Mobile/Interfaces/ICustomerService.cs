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
        Task<IEnumerable<CustomerCommodityModel>> FindCustomerCommodites();
        Task<IEnumerable<CustomerLocationModel>> FindCustomerLocations();
    }
}
