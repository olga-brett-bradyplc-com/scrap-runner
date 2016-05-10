namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("QueueItem")]
    public class QueueItemModel
    {
        [AutoIncrement, PrimaryKey]
        public long? RecordId { get; set; }
        public string RecordType { get; set; }
        public string SerializedRecord { get; set; }
        public QueueItemVerb Verb { get; set; }
        public string DataService { get; set; }
        public string IdType { get; set; }
        public string SerializedId { get; set; }
    }

    public enum QueueItemVerb
    {
        Create,
        Update,
        Delete,
        ChangeSet
    }
}