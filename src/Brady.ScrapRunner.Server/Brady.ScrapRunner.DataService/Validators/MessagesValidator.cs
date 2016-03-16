using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
 
    public class MessagesValidator :
         AbstractValidator<Messages>, IRequireCrudingDataServiceRepository
    {
        ICrudingDataServiceRepository _repository;

        public MessagesValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
            RuleFor(x => x.MsgId).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CreateDateTime).NotEmpty();
            RuleFor(x => x.SenderId).NotEmpty();
            RuleFor(x => x.ReceiverId).NotEmpty();
            RuleFor(x => x.Ack).NotEmpty();
            RuleFor(x => x.MsgThread).GreaterThanOrEqualTo(0);
            RuleFor(x => x.SenderName).NotEmpty();
            RuleFor(x => x.ReceiverName).NotEmpty();
            RuleFor(x => x.Urgent).NotEmpty();
            RuleFor(x => x.Processed).NotEmpty();
            RuleFor(x => x.DeleteFlag).NotEmpty();
        }

        public void SetRepository(ICrudingDataServiceRepository repository)
        {
            _repository = repository;
        }
    }
}
