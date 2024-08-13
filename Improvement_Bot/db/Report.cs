using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json.Serialization;

namespace Improvement_Bot
{
    public class Report
    {
        [Key]
        public int? id_Report { get; set; }
        public int? id_Order { get; set; }

        public string Text { get; set; }
        public bool Accepted { get; set; }

        public string? Manager_Comment { get; set; }
        public DateTime Date_Of_Writing { get; set; }
        
        public string? Header { get; set; }

        public virtual Order? id_OrderNavigation { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Report_Image> Images { get; set; } = new List<Report_Image>();


    }

}
