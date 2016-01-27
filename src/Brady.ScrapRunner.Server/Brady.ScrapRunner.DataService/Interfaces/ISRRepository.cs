using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Support.NHibernate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Interfaces
{
    public interface ISRRepository : IDataServiceRepository, IDisposable, ICrudingDataServiceRepository
    {
    }
}
