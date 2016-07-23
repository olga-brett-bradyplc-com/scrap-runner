using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class HistTripSegmentContainerValidator :
        AbstractValidator<HistTripSegmentContainer>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public HistTripSegmentContainerValidator()
        {
            RuleFor(x => x.HistSeqNo).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
            RuleFor(x => x.TripSegContainerSeqNumber).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
