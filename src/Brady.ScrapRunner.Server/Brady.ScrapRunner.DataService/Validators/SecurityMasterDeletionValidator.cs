using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;


namespace Brady.ScrapRunner.DataService.Validators
{

    public class SecurityMasterDeletionValidator :
        AbstractValidator<SecurityMaster>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public SecurityMasterDeletionValidator()
        {
            RuleFor(x => x.SecurityFunction).NotEmpty();
            RuleFor(x => x.SecurityLevel).NotEmpty();
            RuleFor(x => x.SecurityProgram).NotEmpty();
            RuleFor(x => x.SecurityType).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
