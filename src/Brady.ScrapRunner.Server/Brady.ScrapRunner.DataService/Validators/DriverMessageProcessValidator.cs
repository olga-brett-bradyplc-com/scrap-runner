using System.Collections.Generic;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using NHibernate.Criterion;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverMessageProcessValidator :
       AbstractValidator<DriverMessageProcess>,
       IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverMessageProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            //RuleFor(x => x.ActionDateTime).NotEmpty();
            //RuleFor(x => x.SenderId).NotEmpty();
            //RuleFor(x => x.MessageId).GreaterThanOrEqualTo(0);
            //RuleFor(x => x.MessageText).NotEmpty();
        }

    }
}
