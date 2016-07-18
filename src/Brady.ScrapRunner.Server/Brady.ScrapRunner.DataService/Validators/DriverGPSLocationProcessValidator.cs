using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverGPSLocationProcessValidator :
       AbstractValidator<DriverGPSLocationProcess>,
       IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverGPSLocationProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.GPSID).NotEmpty();
            RuleFor(x => x.ActionDateTime).NotEmpty();
            RuleFor(x => x.Latitude).NotEmpty();
            RuleFor(x => x.Longitude).NotEmpty();
        }

    }
}
