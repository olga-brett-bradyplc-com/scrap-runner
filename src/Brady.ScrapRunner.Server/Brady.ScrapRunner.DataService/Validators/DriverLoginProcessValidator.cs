using System.Collections.Generic;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using NHibernate.Criterion;


namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class DriverLoginProcessValidator :
         AbstractValidator<DriverLoginProcess>,
         IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }

        public DriverLoginProcessValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.Odometer).GreaterThan(0);
            // Here for now, possibly break out as separate call
            //RuleFor(x => x.CodeListVersion).NotEmpty();
            // Here for now, probably break out as separate call
            //RuleFor(x => x.LastContainerMasterUpdate).NotEmpty();
            // Here for now, possibly break out as separate call
            //RuleFor(x => x.LastTerminalMasterUpdate).NotEmpty();
            // Eg: 1033 = English, USA
            //     2058 = Spanish, Mexico
            // Is this a device preference or could it be a user preference (membership?)
            //RuleFor(x => x.LocaleCode).NotEmpty().IsIn( new object[]
            //    {
            //        "1033",
            //        "2058"
            //    }
            //);
            // How is this used?
            //RuleFor(x => x.OverrideFlag).NotEmpty();

        }

    }
}
