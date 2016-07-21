using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;


namespace Brady.ScrapRunner.DataService.Validators
{
    
    public class TripTypeBasicDetailsValidator :
          AbstractValidator<TripTypeBasicDetails>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public TripTypeBasicDetailsValidator()
        {
            RuleFor(x => x.ContainerType).NotEmpty();
            RuleFor(x => x.TripTypeCode).NotEmpty();
            // NOTE: SeqNo is the identity(1,1) column
            //RuleFor(x => x.SeqNo).Equals(0);
        }
    }
}
