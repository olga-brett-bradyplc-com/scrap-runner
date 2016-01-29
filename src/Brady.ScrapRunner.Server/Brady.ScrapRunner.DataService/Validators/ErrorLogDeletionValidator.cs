﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{

    public class ErrorLogDeletionValidator :
        AbstractValidator<ErrorLog>,  IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ErrorLogDeletionValidator()
        {
            RuleFor(x => x.ErrorDateTime).NotEmpty();
            RuleFor(x => x.ErrorSeqNo).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
