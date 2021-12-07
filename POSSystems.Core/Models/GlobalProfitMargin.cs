using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("global_profit_margin")]
    public class GlobalProfitMargin : EntityBase
    {
        [Key]
        [Required]
        [Column("profit_margin_id")]
        public Int32? ProfitMarginId { get; set; }

        [Required]
        [Column("profit_margin")]
        public double? ProfitMargin { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string description { get; set; }
    }
}