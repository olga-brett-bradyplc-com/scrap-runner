using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;


namespace Brady.ScrapRunner.DataService.Validators
{
    public class CodeTableProcessDeletionValidator :
         AbstractValidator<CodeTableProcess>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public CodeTableProcessDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}

