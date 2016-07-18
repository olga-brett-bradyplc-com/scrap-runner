using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class TerminalChangeDeletionValidator :
        AbstractValidator<TerminalChange>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public TerminalChangeDeletionValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
