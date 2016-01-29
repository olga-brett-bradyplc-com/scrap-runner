using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class CustomerCommodityDeletionValidator :
        AbstractValidator<CustomerCommodity>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public CustomerCommodityDeletionValidator()
        {
            RuleFor(x => x.CustHostCode).NotEmpty();
            RuleFor(x => x.CustCommodityCode).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
