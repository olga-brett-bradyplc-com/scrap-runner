using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class GPSLocationDeletionValidator :
        AbstractValidator<GPSLocation>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public GPSLocationDeletionValidator()
        {
            RuleFor(x => x.GPSSeqId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
