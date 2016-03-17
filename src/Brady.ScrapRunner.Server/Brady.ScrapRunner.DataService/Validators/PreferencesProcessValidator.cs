﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class PreferencesProcessValidator :
         AbstractValidator<PreferencesProcess>,
         IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public PreferencesProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }

    }
}