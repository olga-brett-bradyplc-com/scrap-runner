using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class EventLogDeletionValidator :
        AbstractValidator<EventLog>,  IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public EventLogDeletionValidator()
        {
            //RuleFor(x => x.EventDateTime).NotEmpty();
            //RuleFor(x => x.EventSeqNo).GreaterThanOrEqualTo(0);
            // NOTE: EventId is the identity(1,1) column
            RuleFor(x => x.EventId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
