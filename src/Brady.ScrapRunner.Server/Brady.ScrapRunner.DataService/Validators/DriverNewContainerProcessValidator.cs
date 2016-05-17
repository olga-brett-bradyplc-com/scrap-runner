using System.Collections.Generic;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using NHibernate.Criterion;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class DriverNewContainerProcessValidator :
       AbstractValidator<DriverNewContainerProcess>,
       IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverNewContainerProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.ContainerNumber).NotEmpty();
            RuleFor(x => x.ContainerType).NotEmpty();
            RuleFor(x => x.ContainerBarcode).NotEmpty();
            RuleFor(x => x.ActionDateTime).NotEmpty();
        }

    }
}
