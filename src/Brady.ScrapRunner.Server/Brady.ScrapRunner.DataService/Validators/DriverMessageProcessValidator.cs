using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverMessageProcessValidator :
       AbstractValidator<DriverMessageProcess>,
       IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverMessageProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            //RuleFor(x => x.ActionDateTime).NotEmpty();
            //RuleFor(x => x.SenderId).NotEmpty();
            //RuleFor(x => x.MessageId).GreaterThanOrEqualTo(0);
            //RuleFor(x => x.MessageText).NotEmpty();
        }

    }
}
