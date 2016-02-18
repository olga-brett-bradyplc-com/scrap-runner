using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class TripSegmentContainerDeletionValidator :
        AbstractValidator<TripSegmentContainer>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripSegmentContainerDeletionValidator()
        {
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
