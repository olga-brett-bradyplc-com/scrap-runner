using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class EmployeeMasterDeletionValidator :
        AbstractValidator<EmployeeMaster>,
        IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public EmployeeMasterDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
