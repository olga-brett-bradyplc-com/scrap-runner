using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class TerminalMasterValidator :
        AbstractValidator<TerminalMaster>,
        IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public TerminalMasterValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }

}
