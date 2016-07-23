using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class HistTripSegmentValidator :
       AbstractValidator<HistTripSegment>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public HistTripSegmentValidator()
        {
            RuleFor(x => x.HistSeqNo).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
