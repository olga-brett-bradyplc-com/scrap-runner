using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class PowerLimitsValidator :
        AbstractValidator<PowerLimits>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public PowerLimitsValidator()
        {
            RuleFor(x => x.PowerSeqNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.ContainerType).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
