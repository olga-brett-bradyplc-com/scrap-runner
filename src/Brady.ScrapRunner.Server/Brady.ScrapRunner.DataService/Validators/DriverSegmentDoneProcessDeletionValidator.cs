using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverSegmentDoneProcessDeletionValidator :
       AbstractValidator<DriverSegmentDoneProcess>,
       IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverSegmentDoneProcessDeletionValidator()
        {
            // NOTE: Deletes not supported
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}

