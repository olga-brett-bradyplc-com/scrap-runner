using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;


namespace Brady.ScrapRunner.DataService.Validators
{
    public class ContainerChangeProcessDeletionValidator :
         AbstractValidator<ContainerChangeProcess>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;
        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public ContainerChangeProcessDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}
