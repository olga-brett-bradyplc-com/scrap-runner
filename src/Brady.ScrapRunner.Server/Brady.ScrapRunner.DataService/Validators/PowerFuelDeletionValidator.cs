using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class PowerFuelDeletionValidator :
         AbstractValidator<PowerFuel>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public PowerFuelDeletionValidator()
        {
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.PowerFuelSeqNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
