using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverImageProcessDeletionValidator :
        AbstractValidator<DriverImageProcess>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverImageProcessDeletionValidator()
        {
            // NOTE: Deletes not supported
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

    }
}
