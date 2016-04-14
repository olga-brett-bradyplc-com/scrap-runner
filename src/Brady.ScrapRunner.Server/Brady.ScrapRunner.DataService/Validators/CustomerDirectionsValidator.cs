using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
   public class CustomerDirectionsValidator :
        AbstractValidator<CustomerDirections>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public CustomerDirectionsValidator()
        {
            RuleFor(x => x.CustHostCode).NotEmpty();
            RuleFor(x => x.DirectionsSeqNo).GreaterThanOrEqualTo<CustomerDirections, short>(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
