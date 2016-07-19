using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class GPSLocationValidator :
        AbstractValidator<GPSLocation>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public GPSLocationValidator()
        {
            RuleFor(x => x.GPSSeqId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.TerminalId).NotEmpty();
            RuleFor(x => x.RegionId).NotEmpty();
            RuleFor(x => x.GPSID).NotEmpty();
            RuleFor(x => x.GPSDateTime).NotEmpty();
            RuleFor(x => x.GPSLatitude).NotEmpty();
            RuleFor(x => x.GPSLongitude).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
