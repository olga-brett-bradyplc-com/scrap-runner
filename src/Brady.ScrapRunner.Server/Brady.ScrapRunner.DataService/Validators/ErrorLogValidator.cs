using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class ErrorLogValidator :
        AbstractValidator<ErrorLog>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ErrorLogValidator()
        {
            // TODO:  Does this validator need kick in?  Do I distinguish update vs insert?
            // RuleFor(x => x.ErrorId).NotEmpty().When();
            RuleFor(x => x.ErrorDateTime).NotEmpty();
            RuleFor(x => x.ErrorSeqNo).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
