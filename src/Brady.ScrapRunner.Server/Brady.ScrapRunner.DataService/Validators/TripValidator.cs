using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{


    public class TripValidator :
        AbstractValidator<Trip>,
        IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripValidator()
        {
            RuleFor(x => x.TripNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}

