using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class CustomerLocationValidator :
        AbstractValidator<CustomerLocation>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public CustomerLocationValidator()
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
