using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class TripSegmentTimeValidator :
        AbstractValidator<TripSegmentTime>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public TripSegmentTimeValidator()
        {
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
            RuleFor(x => x.SeqNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TimeType).NotEmpty();
            RuleFor(x => x.SegmentTime).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
