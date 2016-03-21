using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Mappings
{

    public class MessagesMap : ClassMapping<Messages>
    {
        public MessagesMap()
        {
            Table("Messages");

            Id(x => x.MsgId, m =>
            {
                m.UnsavedValue(0);
                m.Generator(Generators.Identity);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("MsgId");
                m.Insert(false);
                m.Update(false);
                m.Generated(PropertyGeneration.Never);
            });

            Property(x => x.TerminalId);
            Property(x => x.CreateDateTime);
            Property(x => x.SenderId);
            Property(x => x.ReceiverId);
            Property(x => x.MsgText);
            Property(x => x.Ack);
            Property(x => x.MsgThread);
            Property(x => x.Area);
            Property(x => x.SenderName);
            Property(x => x.ReceiverName);
            Property(x => x.Urgent);
            Property(x => x.Processed);
            Property(x => x.MsgSource);
            Property(x => x.DeleteFlag);
        }
    }
}
