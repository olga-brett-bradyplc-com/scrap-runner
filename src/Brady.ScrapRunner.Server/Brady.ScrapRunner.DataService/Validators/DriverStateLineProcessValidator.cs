﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Process;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverStateLineProcessValidator :
         AbstractValidator<DriverStateLineProcess>,
         IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverStateLineProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.Odometer).GreaterThan(0);
            RuleFor(x => x.ActionDateTime).NotEmpty();
            RuleFor(x => x.State).NotEmpty();
            RuleFor(x => x.Country).NotEmpty();
        }

    }
}
