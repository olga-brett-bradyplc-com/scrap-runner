using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class CodeTableValidator :
        AbstractValidator<CodeTable>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public CodeTableValidator()
        {
            RuleFor(x => x.CodeName).NotEmpty();
            RuleFor(x => x.CodeValue).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
