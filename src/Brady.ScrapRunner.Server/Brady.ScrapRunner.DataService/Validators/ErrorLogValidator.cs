using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class ErrorLogValidator :
        AbstractValidator<ErrorLog>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public ErrorLogValidator()
        {
            RuleFor(x => x.ErrorDateTime).NotEmpty();
            RuleFor(x => x.ErrorSeqNo).GreaterThanOrEqualTo(0);
            // NOTE: ErrorId is the identity(1,1) column
            //RuleFor(x => x.ErrorId).Equals(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
