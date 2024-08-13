using System.ComponentModel.DataAnnotations;

namespace  Improvement_Bot

{
    public class Role
    {
        [Key]
        public int id_Role { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
