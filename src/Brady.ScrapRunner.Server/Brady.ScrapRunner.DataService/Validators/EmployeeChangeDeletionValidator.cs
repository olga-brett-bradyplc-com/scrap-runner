﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class EmployeeChangeDeletionValidator :
         AbstractValidator<EmployeeChange>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public EmployeeChangeDeletionValidator()
        {
            RuleFor(x => x.ActionFlag).NotEmpty();
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.LoginId).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.RegionId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
