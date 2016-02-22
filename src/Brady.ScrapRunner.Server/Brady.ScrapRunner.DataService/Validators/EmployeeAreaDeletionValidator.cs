﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class EmployeeAreaDeletionValidator :
        AbstractValidator<EmployeeArea>,
        IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public EmployeeAreaDeletionValidator()
        {
            RuleFor(x => x.AreaId).NotEmpty();
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
