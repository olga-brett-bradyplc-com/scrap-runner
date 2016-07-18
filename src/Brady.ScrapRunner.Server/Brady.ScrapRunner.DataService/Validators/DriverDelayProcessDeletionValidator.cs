using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverDelayProcessDeletionValidator :
       AbstractValidator<DriverDelayProcess>,
       IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverDelayProcessDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}
