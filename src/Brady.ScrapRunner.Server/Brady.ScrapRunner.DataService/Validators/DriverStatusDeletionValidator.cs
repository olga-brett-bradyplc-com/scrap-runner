using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverStatusDeletionValidator : AbstractValidator<DriverStatus>,
        IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public DriverStatusDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
