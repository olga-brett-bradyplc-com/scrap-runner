﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
        public class CommodityMasterProcessValidator :
         AbstractValidator<CommodityMasterProcess>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;
        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public CommodityMasterProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}
