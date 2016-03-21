using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;


namespace Brady.ScrapRunner.DataService.Validators
{

    public class TripTypeBasicDetailsDeletionValidator :
         AbstractValidator<TripTypeBasicDetails>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripTypeBasicDetailsDeletionValidator()
        {
            RuleFor(x => x.ContainerType).NotEmpty();
            RuleFor(x => x.SeqNo).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripTypeCode).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
