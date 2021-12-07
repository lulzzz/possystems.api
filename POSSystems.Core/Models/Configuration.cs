using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("configuration")]
    public class Configuration : EntityBase
    {
        [Key]
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("config_code")]
        public string ConfigCode { get; set; }

        [Required]
        [Column("config_value")]
        public string ConfigValue { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}