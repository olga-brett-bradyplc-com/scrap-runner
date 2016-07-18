using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class CodeTableDeletionValidator :
        AbstractValidator<CodeTable>,  IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public CodeTableDeletionValidator()
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
