using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class PreferenceDeletionValidator :
        AbstractValidator<Preference>,
        IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public PreferenceDeletionValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
            RuleFor(x => x.Parameter).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
