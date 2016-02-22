using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
     public class ContainerQuantityDeletionValidator :
           AbstractValidator<ContainerQuantity>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ContainerQuantityDeletionValidator()
        {
            RuleFor(x => x.CustHostCode).NotEmpty();
            RuleFor(x => x.CustSeqNo).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
