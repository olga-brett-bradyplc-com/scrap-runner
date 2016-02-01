using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class EventLogDeletionValidator :
        AbstractValidator<EventLog>,  IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public EventLogDeletionValidator()
        {
            RuleFor(x => x.EventDateTime).NotEmpty();
            RuleFor(x => x.EventSeqNo).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
