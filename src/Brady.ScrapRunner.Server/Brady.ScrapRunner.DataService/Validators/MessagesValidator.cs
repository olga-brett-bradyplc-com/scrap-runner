using BWF.DataServices.Support.NHibernate.Interfaces;
using FluentValidation;
using Brady.ScrapRunner.Domain.Models;

namespace Brady.ScrapRunner.DataService.Validators
{
    public class MessagesValidator :
         AbstractValidator<Messages>, IRequireCrudingDataServiceRepository
    {
        private ICrudingDataServiceRepository _repository;

        public MessagesValidator()
        {
            RuleFor(x => x.TerminalId).NotEmpty();
            // NOTE: MsgId is the identity(1,1) column
            //RuleFor(x => x.MsgId).Equals(0);
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
