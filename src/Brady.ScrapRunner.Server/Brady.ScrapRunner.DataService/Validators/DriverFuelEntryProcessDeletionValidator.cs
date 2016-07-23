using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverFuelEntryProcessDeletionValidator :
    AbstractValidator<DriverFuelEntryProcess>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverFuelEntryProcessDeletionValidator()
        {
            // NOTE: Deletes not supported
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

    }
}
