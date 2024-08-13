using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;


namespace Improvement_Bot
{
    public partial class Order
    {
        [Key]
        public int? id_Order { get; set; } = null!;
        public int? id_Supervisor { get; set; } = null!;
        public int? id_Executor { get; set; } = null!;
        public bool? Accepted { get; set; } = null!;
        public string? Text { get; set; } = null!;
        public string? Header { get; set; }
        public DateTime? Date_of_Issue { get; set; }
        public DateTime? Deadline { get; set; }

        public virtual User? id_ExecutorNavigation { get; set; } = null!;
        public virtual User? id_SupervisorNavigation { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
 


    }
}
