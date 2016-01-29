using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class ContainerMasterDeletionValidator :
        AbstractValidator<ContainerMaster>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ContainerMasterDeletionValidator()
        {
            RuleFor(x => x.ContainerNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
