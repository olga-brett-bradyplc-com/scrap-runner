using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class PowerMasterValidator :
        AbstractValidator<PowerMaster>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public PowerMasterValidator()
        {
            RuleFor(x => x.PowerId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }

}
