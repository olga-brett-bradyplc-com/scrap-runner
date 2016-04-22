using System.Collections.Generic;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using NHibernate.Criterion;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverContainerDoneProcessDeletionValidator :
       AbstractValidator<DriverContainerDoneProcess>,
       IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverContainerDoneProcessDeletionValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
        }
    }
}
