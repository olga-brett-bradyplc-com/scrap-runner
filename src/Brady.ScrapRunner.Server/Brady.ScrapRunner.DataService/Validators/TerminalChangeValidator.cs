using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class TerminalChangeValidator :
        AbstractValidator<TerminalChange>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TerminalChangeValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
