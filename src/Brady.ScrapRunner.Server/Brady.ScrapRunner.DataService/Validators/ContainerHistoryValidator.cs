using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;
using NHibernate.Bytecode;

namespace Brady.ScrapRunner.DataService.Validators
{ 
    public class ContainerHistoryValidator :
    AbstractValidator<ContainerHistory>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public ContainerHistoryValidator()
        {
            RuleFor(x => x.ContainerNumber).NotEmpty();
            RuleFor(x => x.ContainerSeqNumber).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
