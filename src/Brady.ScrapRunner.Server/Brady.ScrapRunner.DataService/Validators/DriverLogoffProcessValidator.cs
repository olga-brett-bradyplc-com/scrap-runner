﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;


namespace Brady.ScrapRunner.DataService.Validators
{

    public class DriverLogoffProcessValidator :
         AbstractValidator<DriverLogoffProcess>,
         IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverLogoffProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.Odometer).GreaterThan(0);
            RuleFor(x => x.ActionDateTime).NotEmpty();
        }
    }
}
