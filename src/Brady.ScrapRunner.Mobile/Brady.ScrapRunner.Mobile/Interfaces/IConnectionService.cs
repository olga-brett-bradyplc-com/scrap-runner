using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.PortableClients.Interfaces;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IConnectionService<T> where T : class
    {
        bool CreateConnection(string username, string password, string dataService = null);
        void DeleteConnection();
        T GetConnection();
    }
}
