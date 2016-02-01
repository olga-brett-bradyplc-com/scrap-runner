namespace Brady.ScrapRunner.Mobile.Models
{
    using SQLite.Net.Attributes;

    [Table("EmployeeMaster")]
    public class EmployeeMasterModel
    {
        [PrimaryKey]
        public string EmployeeId { get; set; }
        public string TerminalId { get; set; }
        public string AreaId { get; set; }
        public string RegionId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
    }
}