using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverContainerActionProcessValidator :
       AbstractValidator<DriverContainerActionProcess>,
       IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverContainerActionProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.ActionType).NotEmpty();
            RuleFor(x => x.ActionDateTime).NotEmpty();
            //Exception actions will have no container number
            //RuleFor(x => x.ContainerNumber).NotEmpty();
            //RuleFor(x => x.ContainerContents).NotEmpty();
        }
    }
}
