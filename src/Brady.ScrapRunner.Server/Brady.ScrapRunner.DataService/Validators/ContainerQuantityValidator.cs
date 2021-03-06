﻿using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
        public class ContainerQuantityValidator :
         AbstractValidator<ContainerQuantity>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public ContainerQuantityValidator()
        {
            RuleFor(x => x.CustHostCode).NotEmpty();
            RuleFor(x => x.CustSeqNo).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
