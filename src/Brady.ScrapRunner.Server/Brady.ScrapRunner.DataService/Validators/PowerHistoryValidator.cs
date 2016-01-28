using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class PowerHistoryValidator :
        AbstractValidator<PowerHistory>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public PowerHistoryValidator()
        {
            RuleFor(x => x.PowerId).NotEmpty();
            RuleFor(x => x.PowerSeqNumber).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }

}
