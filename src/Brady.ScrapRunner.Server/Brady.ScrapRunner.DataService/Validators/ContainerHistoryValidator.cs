using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{ 
    public class ContainerHistoryValidator :
    AbstractValidator<ContainerHistory>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public ContainerHistoryValidator()
        {
            RuleFor(x => x.ContainerNumber).NotEmpty();
            RuleFor(x => x.ContainerSeqNumber).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
