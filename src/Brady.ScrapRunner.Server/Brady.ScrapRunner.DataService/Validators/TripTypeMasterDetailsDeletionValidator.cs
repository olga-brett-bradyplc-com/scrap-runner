using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class TripTypeMasterDetailsDeletionValidator :
        AbstractValidator<TripTypeMasterDetails>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public TripTypeMasterDetailsDeletionValidator()
        {
            RuleFor(x => x.AccessorialCode).NotEmpty();
            RuleFor(x => x.TripTypeCode).NotEmpty();
            RuleFor(x => x.TripTypeSeqNumber).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
