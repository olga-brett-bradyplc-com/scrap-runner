using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class TripSegmentContainerValidator :
        AbstractValidator<TripSegmentContainer>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public TripSegmentContainerValidator()
        {
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
            RuleFor(x => x.TripSegContainerSeqNumber).GreaterThanOrEqualTo<TripSegmentContainer, short>(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
