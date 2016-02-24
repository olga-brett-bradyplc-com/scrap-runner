using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class HistTripDeletionValidator :
        AbstractValidator<HistTrip>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public HistTripDeletionValidator()
        {
            RuleFor(x => x.HistSeqNo).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
