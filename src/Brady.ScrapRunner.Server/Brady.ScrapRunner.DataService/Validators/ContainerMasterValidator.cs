using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using NHibernate.Bytecode;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class ContainerMasterValidator :
        AbstractValidator<ContainerMaster>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ContainerMasterValidator()
        {
            RuleFor(x => x.ContainerNumber).NotEmpty();
            RuleFor(x => x.ContainerQtyInIDFlag).NotEmpty();  // A not null field!
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
