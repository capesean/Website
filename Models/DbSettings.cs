using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class DbSettings
    {
        [Key, Required]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = true), MaxLength(50)]
        public string TestField { get; set; }

        public DbSettings()
        {
            this.Id = Guid.Empty;
        }
    }
}
