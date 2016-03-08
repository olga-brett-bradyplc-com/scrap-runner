using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.Domain;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Support.NHibernate.Abstract;
using BWF.Globalisation.Interfaces;
using BWF.Hosting.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace Brady.ScrapRunner.DataService.Concrete
{
    public class SRRepository : ConventionalDatabaseDataServiceRepository, ISRRepository
    {
        public SRRepository(
               IHostConfiguration hostConfiguration,
               IGlobalisationProvider globalisationProvider,
               IAuthorisation authorisation,
               IMetadataProvider metadataProvider)
            : base(
            hostConfiguration,
            globalisationProvider,
            authorisation,
            Enumerable.Empty<string>(),
            metadataProvider,
            Constants.ScrapRunner, "dbo")
        {
        }

        public ISession OpenSession()
        {
            return sessionFactory.OpenSession();
        }

    }
}
