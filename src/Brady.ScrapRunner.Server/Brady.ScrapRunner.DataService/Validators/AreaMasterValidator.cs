using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class AreaMasterValidator :
        AbstractValidator<AreaMaster>,
        IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public AreaMasterValidator()
        {
            RuleFor(x => x.AreaId).NotEmpty();
            RuleFor(x => x.TerminalId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
