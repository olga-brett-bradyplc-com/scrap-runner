using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class CustomerLocationDeletionValidator :
        AbstractValidator<CustomerLocation>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public CustomerLocationDeletionValidator()
        {
            RuleFor(x => x.CustHostCode).NotEmpty();
            RuleFor(x => x.CustLocation).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
