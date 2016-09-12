using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class EmployeePreferencesValidator :
         AbstractValidator<EmployeePreferences>,
         IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public EmployeePreferencesValidator()
        {
            RuleFor(x => x.RegionId).NotEmpty();
            RuleFor(x => x.TerminalId).NotEmpty();
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Parameter).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
