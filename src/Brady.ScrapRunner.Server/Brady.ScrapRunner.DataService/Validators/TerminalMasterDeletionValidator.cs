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
    public class TerminalMasterDeletionValidator : AbstractValidator<TerminalMaster>,
        IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TerminalMasterDeletionValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
