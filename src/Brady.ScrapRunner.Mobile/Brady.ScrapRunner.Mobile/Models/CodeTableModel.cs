namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using SQLite.Net.Attributes;

    [Table("CodeTable")]
    public class CodeTableModel
    {
        [PrimaryKey, MaxLength(51)]
        public string CompositeKey => CodeName + ";" + CodeValue;

        [MaxLength(20)]
        public string CodeName { get; set; }

        public int? CodeSeq { get; set; }

        [MaxLength(30)]
        public string CodeValue { get; set; }

        [MaxLength(60)]
        public string CodeDisp1 { get; set; }

        [MaxLength(60)]
        public string CodeDisp2 { get; set; }

        [MaxLength(60)]
        public string CodeDisp3 { get; set; }

        [MaxLength(60)]
        public string CodeDisp4 { get; set; }

        [MaxLength(60)]
        public string CodeDisp5 { get; set; }

        [MaxLength(60)]
        public string CodeDisp6 { get; set; }
    }
}
