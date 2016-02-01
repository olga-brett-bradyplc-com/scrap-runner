using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class ContainerChangeValidator :
        AbstractValidator<ContainerChange>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ContainerChangeValidator()
        {
            RuleFor(x => x.ContainerNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
