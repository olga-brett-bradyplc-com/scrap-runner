using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class TripTypeBasicValidator :
        AbstractValidator<TripTypeBasic>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripTypeBasicValidator()
        {
            RuleFor(x => x.TripTypeCode).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}

