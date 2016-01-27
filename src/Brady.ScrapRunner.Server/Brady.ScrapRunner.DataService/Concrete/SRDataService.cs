using Brady.ScrapRunner.DataService.Interfaces;
using Brady.ScrapRunner.Domain;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Interfaces;
using BWF.DataServices.Support.NHibernate.Abstract;
using BWF.Globalisation.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Concrete
{
    public class SRDataService :
        ConventionalDatabaseDataService<SRDataService>,
        IDataService
    {
        public SRDataService(
            IEnumerable<IRecordType> recordTypes,
            IGlobalisationProvider globalisationProvider,
            ISRRepository srRepository,
            IMetadataProvider metadataProvider)
            : base(
            Constants.ScrapRunner,
            globalisationProvider,
            srRepository as DatabaseDataServiceRepository,
            recordTypes,
            metadataProvider)
        {
            ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;
        }
    }
}
