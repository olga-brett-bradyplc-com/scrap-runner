using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class DriverDelayValidator :
        AbstractValidator<DriverDelay>,
        IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public DriverDelayValidator()
        {
            RuleFor(x => x.DriverId).NotEmpty();
            RuleFor(x => x.DelaySeqNumber).NotEmpty();
            RuleFor(x => x.TripNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
