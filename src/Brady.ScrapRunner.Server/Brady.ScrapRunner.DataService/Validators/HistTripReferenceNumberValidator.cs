﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class HistTripReferenceNumberValidator :
        AbstractValidator<HistTripReferenceNumber>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public HistTripReferenceNumberValidator()
        {
            RuleFor(x => x.HistSeqNo).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSeqNumber).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TripRefNumberDesc).NotEmpty();
            RuleFor(x => x.TripRefNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
