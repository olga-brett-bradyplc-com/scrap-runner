using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class CodeTableHdrValidator :
        AbstractValidator<CodeTableHdr>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public CodeTableHdrValidator()
        {
            RuleFor(x => x.CodeName).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
