using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class TripTypeBasicDetailsDeletionValidator :
         AbstractValidator<TripTypeBasicDetails>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public TripTypeBasicDetailsDeletionValidator()
        {
            //RuleFor(x => x.TripTypeCode).NotEmpty();
            //RuleFor(x => x.ContainerType).NotEmpty();
            // NOTE: SeqNo is the identity(1,1) column
            RuleFor(x => x.SeqNo).NotEmpty();
        }
    }
}
