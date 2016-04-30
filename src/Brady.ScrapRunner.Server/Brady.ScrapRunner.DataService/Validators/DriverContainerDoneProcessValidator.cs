using System.Collections.Generic;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using NHibernate.Criterion;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverContainerDoneProcessValidator :
       AbstractValidator<DriverContainerDoneProcess>,
       IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverContainerDoneProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.TripNumber).NotEmpty();
            RuleFor(x => x.TripSegNumber).NotEmpty();
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.ContainerNumber).NotEmpty();
            RuleFor(x => x.ActionDateTime).NotEmpty();
        }
    }
}
