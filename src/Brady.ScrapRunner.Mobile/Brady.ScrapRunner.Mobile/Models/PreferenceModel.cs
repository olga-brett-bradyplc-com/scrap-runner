namespace Brady.ScrapRunner.Mobile.Models
{
    using Domain.Models;
    using SQLite.Net.Attributes;

    [Table("Preference")]
    public class PreferenceModel : Preference
    {
        [PrimaryKey]
        public override string Id => $"{Parameter};{TerminalId}";

        [Indexed]
        public override string Parameter { get; set; }
    }
}
