namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("QueueItem")]
    public class QueueItemModel
    {
        [AutoIncrement, PrimaryKey]
        public long? Id { get; set; }
        public string RecordType { get; set; }
        public string SerializedRecord { get; set; }
        public QueueItemVerb Verb { get; set; }
    }

    public enum QueueItemVerb
    {
        Create,
        Update,
        Delete
    }
}