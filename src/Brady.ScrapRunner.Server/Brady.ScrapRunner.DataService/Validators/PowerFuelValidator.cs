using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class PowerFuelValidator :
        AbstractValidator<PowerFuel>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public PowerFuelValidator()
        {
            RuleFor(x => x.PowerFuelSeqNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.TripNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
