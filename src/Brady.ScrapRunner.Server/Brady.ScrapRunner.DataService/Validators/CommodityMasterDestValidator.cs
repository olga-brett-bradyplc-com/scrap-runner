using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    
    public class CommodityMasterDestValidator :
         AbstractValidator<CommodityMasterDest>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public CommodityMasterDestValidator()
        {
            RuleFor(x => x.CommodityCode).NotEmpty();
            RuleFor(x => x.DestTerminalId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
