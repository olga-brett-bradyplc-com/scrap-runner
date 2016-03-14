using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class CustomerDirectionsDeletionValidator :
           AbstractValidator<CustomerDirections>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public CustomerDirectionsDeletionValidator()
        {
            RuleFor(x => x.CustHostCode).NotEmpty();
            RuleFor(x => x.DirectionsSeqNo).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
