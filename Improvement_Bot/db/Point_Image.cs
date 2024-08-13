using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Improvement_Bot.db
{
    public partial class Point_Image
    {
        [Key]
        public int? id_Point_Image { get; set; }
        public string Name;
        public Guid Point_Image_GUID { get; set; }
        public byte[] Image { get; set; }
        public virtual ICollection<Point> Points { get; set; } = new List<Point>();


    }

}
