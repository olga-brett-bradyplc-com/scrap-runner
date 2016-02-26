using System;
using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;


namespace Brady.ScrapRunner.DataService.Validators
{
   
    public class TripTypeMasterDeletionValidator :
        AbstractValidator<TripTypeMaster>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public TripTypeMasterDeletionValidator()
        {
            RuleFor(x => x.TripTypeCode).NotEmpty();
            RuleFor(x => x.TripTypeSeqNumber).GreaterThanOrEqualTo(0);
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
