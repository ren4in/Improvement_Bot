using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Improvement_Bot.db;
using Point = Improvement_Bot.db.Point;
namespace Improvement_Bot
{

    public partial class User
        {
            [Key]
          //  [JsonIgnore]
            public int? id_User { get; set; }

            public int? id_Role { get; set; }

            public string? FirstName { get; set; } = null!;

            public string? MiddleName { get; set; }

            public string? LastName { get; set; } = null!;

            public string? Login { get; set; } = null!;
            public string? Phone { get; set; } = null!;

            public string? Password { get; set; } = null!;
            [JsonIgnore]
            public virtual ICollection<Order> Orders1 { get; set; } = new List<Order>();
        public virtual ICollection<Order> Orders2 { get; set; } = new List<Order>();

        public virtual ICollection<Point> Points { get; set; } = new List<Point>();


        public virtual Role? id_RoleNavigation { get; set; } = null!;



        }
    }
