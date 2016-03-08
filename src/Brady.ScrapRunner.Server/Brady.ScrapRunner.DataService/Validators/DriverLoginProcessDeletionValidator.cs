using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;


namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverLoginProcessDeletionValidator :
        AbstractValidator<DriverLoginProcess>,
        IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;
        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverLoginProcessDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

    }
}
