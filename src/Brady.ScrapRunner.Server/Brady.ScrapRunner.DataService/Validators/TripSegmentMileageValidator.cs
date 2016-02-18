using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class TripSegmentMileageValidator :
        AbstractValidator<TripSegmentMileage>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripSegmentMileageValidator()
        {
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
            RuleFor(x => x.TripSegMileageSeqNumber).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}

