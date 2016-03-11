using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class TripPointsValidator :
         AbstractValidator<TripPoints>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripPointsValidator()
        {
            RuleFor(x => x.TripPointsHostCode1).NotEmpty();
            RuleFor(x => x.TripPointsHostCode2).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
