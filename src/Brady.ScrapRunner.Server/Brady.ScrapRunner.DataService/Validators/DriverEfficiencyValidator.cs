﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverEfficiencyValidator :
        AbstractValidator<DriverEfficiency>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public DriverEfficiencyValidator()
        {
            RuleFor(x => x.TripDriverId).NotEmpty();
            RuleFor(x => x.TripNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
