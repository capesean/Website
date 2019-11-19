using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public partial class User
    {
        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string LastName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; private set; }

        [Required]
        public bool Enabled { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
        }

        public override string ToString()
        {
            return FirstName;
        }
    }
}
