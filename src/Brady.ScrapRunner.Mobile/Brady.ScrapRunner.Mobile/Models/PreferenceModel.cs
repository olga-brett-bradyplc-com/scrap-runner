using System.Reflection;

namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("Preference")]
    public class PreferenceModel
    {
        [PrimaryKey, MaxLength(41)]
        public string CompositeKey => Parameter + ";" + TerminalId;

        [MaxLength(10)]
        public string TerminalId { get; set; }

        [MaxLength(30)]
        public string Parameter { get; set; }
        
        [MaxLength(30)]
        public string ParameterValue { get; set; }

        [MaxLength(30)]
        public string Description { get; set; }
    }
}
