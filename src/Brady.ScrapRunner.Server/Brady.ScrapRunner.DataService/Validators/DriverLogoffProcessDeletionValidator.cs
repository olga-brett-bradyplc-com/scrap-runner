using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;


namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverLogoffProcessDeletionValidator :
        AbstractValidator<DriverLogoffProcess>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;
        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverLogoffProcessDeletionValidator()
        {
            // TODO:  Need a simple failure, deletes not allowed.
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

    }
}
