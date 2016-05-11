using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class DriverMessageProcessMetadata : TypeMetadataProvider<DriverMessageProcess>
    {
        public DriverMessageProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.SenderId);
            StringProperty(x => x.ReceiverId);
            TimeProperty(x => x.ActionDateTime);
            StringProperty(x => x.MessageId);
            IntegerProperty(x => x.MessageThread);
            StringProperty(x => x.MessageText);
            StringProperty(x => x.UrgentFlag);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.SenderId)
            .Property(x => x.ReceiverId)
            .Property(x => x.ActionDateTime)
            .Property(x => x.MessageId)
            .Property(x => x.MessageThread)
            .Property(x => x.MessageText)
            .Property(x => x.UrgentFlag)

            .OrderBy(x => x.EmployeeId);
        }
    }
}
