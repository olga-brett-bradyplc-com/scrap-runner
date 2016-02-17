using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class EventLogValidator :
        AbstractValidator<EventLog>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public EventLogValidator()
        {
            // TODO:  Does this validator need kick in?  Do I distinguish update vs insert?
            // RuleFor(x => x.EventId).NotEmpty().When();
            RuleFor(x => x.EventDateTime).NotEmpty();
            RuleFor(x => x.EventSeqNo).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
